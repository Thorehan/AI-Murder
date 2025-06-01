tools = [
    {
        "type": "function",
        "function": {
            "name": "SearchRecord",
            "description": "Searches for the most similar records to the query using the embedding model.",
            "strict": True,
            "parameters": {
                "type": "object",
                "required": ["query", "top_k"],
                "properties": {
                    "query": {
                        "type": "string",
                        "description": "The search query to find similar records."
                    },
                    "top_k": {
                        "type": "integer",
                        "description": "The number of top similar records to return."
                    }
                },
                "additionalProperties": False
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "ChangeRecord",
            "description": "Updates a record in the database using the embedding model.",
            "strict": True,
            "parameters": {
                "type": "object",
                "required": ["record_id", "new_data"],
                "properties": {
                    "record_id": {
                        "type": "integer",
                        "description": "The ID of the record to be updated."
                    },
                    "new_data": {
                        "type": "string",
                        "description": "The new message to update the record with."
                    }
                },
                "additionalProperties": False
            },
        }
    },
    {
        "type": "function",
        "function": {
            "name": "DeleteRecord",
            "description": "Deletes a record from the database by its ID.",
            "strict": True,
            "parameters": {
                "type": "object",
                "required": ["record_id"],
                "properties": {
                    "record_id": {
                        "type": "integer",
                        "description": "The ID of the record to be deleted."
                    }
                },
                "additionalProperties": False
            },
        }
    },
    {
        "type": "function",
        "function": {
            "name": "AddRecord",
            "description": "Adds a new record to the database.",
            "strict": True,
            "parameters": {
                "type": "object",
                "required": ["new_data"],
                "properties": {
                    "new_data": {
                        "type": "string",
                        "description": "The message to add to the record."
                    }
                },
                "additionalProperties": False
            },
        }
    },
    {
        "type": "function",
        "function": {
            "name": "FlushHistory",
            "description": "Flushes the chat history after your answer, keeping only the system prompt and a summary of the previous conversation.",
            "strict": True,
            "parameters": {
                "type": "object",
                "properties": {},
                "additionalProperties": False
            }
        }
    }
]