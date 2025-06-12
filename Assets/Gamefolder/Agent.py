import os
import json
import csv
import numpy as np
from pydantic import BaseModel, Field
from openai import OpenAI
import pathlib
import Tools



class ThoughtResponse(BaseModel):
    thinking: str = Field(default=None, description="Thoughts of the agent, this is not conveyed to others")
    message: str = Field(default=None, description="Message generated and told to others")
    action: str = Field(default=None, description="Action to be taken by the agent, if any. this will interact with code. you will write where you go here")

class AIAgent:
    def __init__(self, name: str, Client: object, model: str, embedding_model: str, system_prompt: str = ""):
        self.name = name
        self.model = model
        self.embedding_model = embedding_model
        self.client = Client
        self.csv_path = pathlib.Path(f"{self.name}.csv")
        data = ""
        if system_prompt != "":
            data = system_prompt
        else:
            self.sysprompt_path = pathlib.Path(f"sysprompt.txt") if not pathlib.Path(f"{self.name}_sysprompt.txt").exists() else pathlib.Path(f"{self.name}_sysprompt.txt")
            with open(self.sysprompt_path, 'r', encoding='utf-8') as f:
                data = f.read()

        with open(pathlib.Path(f"sysprompt.txt"),  'r', encoding='utf-8') as f2:
            data2 = f2.read()
            data = data2 + data
        self.system_prompt = {"role": "system", "content": data}
        self.chat_history = [self.system_prompt]
        self.test_text = self.load_data_csv()
        self.tools = Tools.tools

    def append_history(self, text, role="user"):
        self.chat_history.append({"role": role, "content": text})

    def load_data_csv(self):
        if self.csv_path.exists():
            with open(self.csv_path, newline='', encoding='utf-8') as csvfile:
                reader = csv.DictReader(csvfile)
                data = []
                for row in reader:
                    item = {"id": int(row["id"]), "content": row["content"]}
                    if "vector" in row and row["vector"]:
                        try:
                            item["vector"] = json.loads(row["vector"])
                        except Exception:
                            item["vector"] = None
                    else:
                        item["vector"] = None
                    data.append(item)
                return data
        return []

    def save_data_csv(self):
        with open(self.csv_path, 'w', newline='', encoding='utf-8') as csvfile:
            fieldnames = ["id", "content", "vector"]
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            writer.writeheader()
            for item in self.test_text:
                writer.writerow({
                    "id": item["id"],
                    "content": item["content"],
                    "vector": json.dumps(item["vector"]) if item.get("vector") is not None else ""
                })

    def GetEmbedding(self, text):
        response = self.client.embeddings.create(
            model=self.embedding_model,
            input=text,
            encoding_format="float"
        )
        if isinstance(text, list):
            return [item.embedding for item in response.data]
        return response.data[0].embedding

    def cosine_similarity(self, a, b):
        a = np.array(a)
        b = np.array(b)
        return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

    def get_or_create_embedding(self, text, id=None):
        item = None
        if id is not None:
            for entry in self.test_text:
                if entry["id"] == id:
                    item = entry
                    break
        if item and item.get("vector") is not None:
            return item["vector"]
        emb = self.GetEmbedding(text)
        if item is not None:
            item["vector"] = emb
            self.save_data_csv()
        return emb

    def SearchRecord(self, search_text, top_k=3):
        contents = [item["content"] for item in self.test_text]
        ids = [item["id"] for item in self.test_text]
        query_embedding = self.GetEmbedding(search_text)
        text_embeddings = [self.get_or_create_embedding(content, id) for content, id in zip(contents, ids)]
        similarities = [self.cosine_similarity(query_embedding, emb) for emb in text_embeddings]
        sorted_indices = np.argsort(similarities)[::-1][:top_k]
        results = [(self.test_text[i], similarities[i]) for i in sorted_indices]
        for item, score in results:
            print(f"ID: {item['id']} | Content: {item['content']} (score: {score:.4f})")
        return results

    def AddRecordFunction(self, args):
        temp = json.loads(args)
        new_record = temp["new_data"]
        if self.test_text:
            new_id = max(item["id"] for item in self.test_text) + 1
        else:
            new_id = 1
        emb = self.GetEmbedding(new_record)
        self.test_text.append({"id": new_id, "content": new_record, "vector": emb})
        self.save_data_csv()
        return f"Record added with ID: {new_id}"

    def ChangeRecordFunction(self, args):
        temp = json.loads(args)
        record_id = temp["record_id"]
        new_data = temp["new_data"]
        for item in self.test_text:
            if item["id"] == record_id:
                item["content"] = new_data
                item["vector"] = self.GetEmbedding(new_data)
                break
        self.save_data_csv()
        pass

    def DeleteRecordFunction(self, args):
        temp = json.loads(args)
        record_id = temp["record_id"]
        for item in self.test_text:
            if item["id"] == record_id:
                item["content"] = "Content Deleted"
                item["vector"] = None
                break
        self.save_data_csv()
        pass

    def FlushEnable(self, args):
        global flush
        flush = True

    def FlushHistory(self):
        global flush
        flush = False
        messages = [msg['content'] for msg in self.chat_history if msg['role'] != 'system']
        if messages:
            summary_prompt = "Summarize the following chat history in a concise paragraph:\n" + "\n".join(messages)
            response = self.client.chat.completions.create(
                model=self.model,
                messages=[{"role": "system", "content": "You are a helpful assistant that summarizes conversations."},
                          {"role": "user", "content": summary_prompt}],
                max_completion_tokens=1024,
                temperature=0.3
            )
            summary = response.choices[0].message.content
            self.chat_history = [self.system_prompt, {"role": "system", "content": f"Summary of previous chat: {summary}"}]
        else:
            self.chat_history = [self.system_prompt]

    def OneShot(self, input, sysmessage):
        response = self.client.chat.completions.create(
            model=self.model,
            messages=[{"role": "system", "content": sysmessage}, {"role": "user", "content": input}],
            max_completion_tokens=2048,
            temperature=0.8,
            tools=self.tools,
            tool_choice="auto",
            response_format={"type": "json_object"}
        )
        data = json.loads(response.choices[0].message.content)
        return ThoughtResponse(**data)

    def GenerateText(self):
        response = self.client.beta.chat.completions.parse(
            model=self.model,
            messages=self.chat_history,
            max_completion_tokens=2048,
            frequency_penalty=0,
            temperature=0.8,
            tools=self.tools,
            tool_choice="auto",
            response_format=ThoughtResponse
        )
        response_message = response.choices[0].message
        toolcall = response_message.tool_calls
        if toolcall:
            self.chat_history.append(response_message)
            function_list = {
                "SearchRecord": self.SearchRecordFunction,
                "ChangeRecord": self.ChangeRecordFunction,
                "DeleteRecord": self.DeleteRecordFunction,
                "AddRecord": self.AddRecordFunction,
                "FlushHistory": self.FlushEnable
            }
            for call in toolcall:
                calling = function_list[call.function.name]
                print(f"Calling function: {call.function.name}")
                args = call.function.arguments
                func_response = calling(args)
                if not isinstance(func_response, str):
                    func_response = json.dumps(func_response, ensure_ascii=False)
                self.chat_history.append(
                    {
                        "role": "tool",
                        "tool_call_id": call.id if hasattr(call, 'id') else None,
                        "content": func_response
                    }
                )
            return self.GenerateText()
        thought_response = response.choices[0].message.content
        try:
            data = json.loads(thought_response)
        except Exception as e:
            print(f"Error parsing response: {e}")
            data = {"thinking": "", "message": thought_response}
            return thought_response
        thought = data.get("thinking", "")
        message = data.get("message", "")
        print(f"Thought: {thought}")
        print(f"Message: {message}")
        return response.choices[0].message.content

    def SearchRecordFunction(self, args):
        temp = json.loads(args)
        if "vector" in temp:
            temp.pop("vector")
        return self.SearchRecord(temp["query"], temp["top_k"])

    def get_response(self, prompt: str):
        """
        Appends the user prompt to chat history and returns the AI's response message.
        """
        self.append_history(prompt, role="user")
        response = self.GenerateText()
        return response
    
    
    buffer = ""
    memoryCompress = "";
    def OneShotGetReply(self, input):
        memory = [{"role": "system", "content": self.system_prompt["content"]}, {"role": "user", "content": self.buffer + input}]
        self.buffer = ""
        self.memoryCompress = self.MemoryCompress(context=memory);
        memory = [{"role": "system", "content": self.system_prompt["content"]}, {"role": "user", "content": self.memoryCompress}]
        memory.append({"role": "user", "content": self.SearchMemoryDatabase(memory)})
        response = self.client.beta.chat.completions.parse(
            model=self.model,
            messages= memory,
            max_completion_tokens=2048,
            temperature=0.8,
            response_format=ThoughtResponse
        )
        data = json.loads(response.choices[0].message.content)
        return ThoughtResponse(**data)
    
    def OneShotAddMemory(self, input):
        self.buffer += "\n"
        self.buffer += input
        return
    
    def MemoryCompress(self, context):
        context = context.copy()
        custom_system_prompt = {"role": "system", "content": """You are a Context Sub-module, you will take the input and add it to previous context.
                                a context message looks like this:
                                '''
                                Summary: while you were doing x, you heard y happened. after that you did z...(this summary can be as long as you need, it have to contains everything that happened, but as you add more and more info, it's ok to forget some of them)
                                LastMessage: enter last input here, untouched.
                                '''
                                """}
        context = [custom_system_prompt if msg['role'] == 'system' else msg for msg in context]
        context.append({"role": "user", "content": self.memoryCompress});
        response = self.client.chat.completions.create(
            model=self.model,
            messages=context,
            max_completion_tokens=2048,
            temperature=0.4,
        )
        return response.choices[0].message.content
        
    
    def SearchMemoryDatabase(self, context):
        # Replace the system prompt in the context with a string of your choice
        
        context = context.copy()
        custom_system_prompt = {"role": "system", "content": "You are a memory Sub-module, you will search the database throughly, and return important information. when adding info, you must add relevant text too, an example is \"while doing x, you hear y happened\" and so on. you can make multiple tool calls to ensure you get all the information needed. in the end, you will return with \"<memory> you can remember y happened when you were doing x\" or similiar response."}
        context = [custom_system_prompt if msg['role'] == 'system' else msg for msg in context]
        
        response = self.client.chat.completions.create(
            model=self.model,
            messages= context,
            max_completion_tokens=2048,
            frequency_penalty=0,
            temperature=0.4,
            tools=self.tools,
            tool_choice="auto"
        )
        response_message = response.choices[0].message
        toolcall = response_message.tool_calls
        if toolcall:
            context.append(response_message)
            function_list = {
                "SearchRecord": self.SearchRecordFunction,
                "ChangeRecord": self.ChangeRecordFunction,
                "DeleteRecord": self.DeleteRecordFunction,
                "AddRecord": self.AddRecordFunction,
            }
            
            tool_responses = []
            for call in toolcall:
                calling = function_list[call.function.name]
                print(f"Calling function: {call.function.name}")
                args = call.function.arguments
                func_response = calling(args)
                if not isinstance(func_response, str):
                    func_response = json.dumps(func_response, ensure_ascii=False)
                tool_responses.append(
                    {
                        "role": "tool",
                        "tool_call_id": call.id,
                        "content": func_response
                    }
                )
            context.extend(tool_responses)
            return self.GenerateText()
        
        return response.choices[0].message.content