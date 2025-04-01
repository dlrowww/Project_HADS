from fastapi import FastAPI, Query
from pymongo import MongoClient
from typing import Optional

app = FastAPI()

client = MongoClient("mongodb://db:27017/")
db = client["food"]
collection = db["restaurants"]

@app.get("/offers")
def get_all_offers():
    data = list(collection.find({}, {"_id": 0}))
    return {"count": len(data), "data": data}

@app.get("/offers/search")
def search_offers(
    minRating: Optional[float] = None,
    maxDeliveryCost: Optional[float] = None,
    minDeliveryValue: Optional[float] = None,
    city: Optional[str] = None
):
    query = {}

    if minRating is not None:
        query["rating"] = {"$gte": minRating}
    if maxDeliveryCost is not None:
        query["delivery_cost"] = {"$lte": maxDeliveryCost}
    if minDeliveryValue is not None:
        query["min_delivery_value"] = {"$gte": minDeliveryValue}
    if city is not None:
        query["city"] = city

    result = list(collection.find(query, {"_id": 0}))
    return {"count": len(result), "data": result}



