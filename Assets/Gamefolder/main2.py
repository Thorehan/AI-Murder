import os
from openai import OpenAI
import pandas as pd
import numpy as np
from pydantic import BaseModel
import Tools
import json
import csv
import json
import pathlib
from fastapi import FastAPI
import Agent as Agent
import uvicorn

app = FastAPI()
api_key = os.environ["OPENAI_API_KEY_SHARE"]
model = "gpt-4.1-nano"
embedding_model = "text-embedding-3-small"
client = OpenAI(api_key=api_key)

class ThoughtResponse(BaseModel):
    thinking: str
    message: str

Storyteller = Agent.AIAgent(name="Storyteller", Client=client, model=model, embedding_model=embedding_model)
Butler = Agent.AIAgent(name="Butler", Client=client, model=model, embedding_model=embedding_model)
Maid = Agent.AIAgent(name="Maid", Client=client, model=model, embedding_model=embedding_model)
Lord = Agent.AIAgent(name="Lord", Client=client, model=model, embedding_model=embedding_model)
Knight = Agent.AIAgent(name="Knight", Client=client, model=model, embedding_model=embedding_model)


@app.get("/getResponse")
def get_response(character: str ,prompt: str):
    if character == "Storyteller":
        response = Storyteller.get_response(prompt)
    elif character == "Butler":
        response = Butler.get_response(prompt)
    elif character == "Maid":
        response = Maid.get_response(prompt)
    elif character == "Lord":
        response = Lord.get_response(prompt)
    elif character == "Knight":
        response = Knight.get_response(prompt)
    else:
        return {"error": "Character not found"}
    return response
    
@app.get("/getAddToMemory")
def get_add_to_memory(character: str, prompt: str, role: str = "user"):
    if character == "Storyteller":
        response = Storyteller.append_history(prompt, role=role)
    elif character == "Butler":
        response = Butler.append_history(prompt, role=role)
    elif character == "Maid":
        response = Maid.append_history(prompt, role=role)
    elif character == "Lord":
        response = Lord.append_history(prompt, role=role)
    elif character == "Knight":
        response = Knight.append_history(prompt, role=role)    
    else:
        return {"error": "Character not found"}
    return response

@app.get("/Storyteller")
def get_story_teller_response(prompt: str):
    response = Storyteller.get_response(prompt)
    return response
@app.get("/Butler")
def get_butler_response(prompt: str):
    response = Butler.get_response(prompt)
    return response
@app.get("/Maid")
def get_maid_response(prompt: str):
    response = Maid.get_response(prompt)
    return response
@app.get("/Lord")
def get_lord_response(prompt: str):
    response = Lord.get_response(prompt)
    return response
@app.get("/Knight")
def get_knight_response(prompt: str):
    response = Knight.get_response(prompt)
    return response
@app.get("/getLocation")
def get_location(character: str):
    return {"location": character}

if __name__ == "__main__":
    uvicorn.run("main2:app", host="0.0.0.0", port=8000, reload=True)