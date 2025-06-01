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
model = "gpt-4o-mini"
embedding_model = "text-embedding-3-small"
client = OpenAI(api_key=api_key)


class ThoughtResponse(BaseModel):
    thinking: str
    message: str



Storyteller = Agent.AIAgent(name="Storyteller", api_key=api_key, model=model, embedding_model=embedding_model)
Butler = Agent.AIAgent(name="Butler", api_key=api_key, model=model, embedding_model=embedding_model)
Maid = Agent.AIAgent(name="Maid", api_key=api_key, model=model, embedding_model=embedding_model)
Lord = Agent.AIAgent(name="Lord", api_key=api_key, model=model, embedding_model=embedding_model)
Knight = Agent.AIAgent(name="Knight", api_key=api_key, model=model, embedding_model=embedding_model)



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


if __name__ == "__main__":
    uvicorn.run("main2:app", host="0.0.0.0", port=8000, reload=True)