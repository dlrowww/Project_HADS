from flask import Flask, jsonify,Response
import json
import pymongo
import requests


app = Flask(__name__)

# Connect to MongoDB
client = pymongo.MongoClient("mongodb://db:27017/")
db = client["food"]
collection = db["restaurants"]

# Core scraping function - calls the official Pyszne API
def real_scrape():
    url = "https://rest.api.eu-central-1.production.jet-external.com/discovery/pl/restaurants/enriched"
    params = {
        "latitude": 51.73747,
        "longitude": 19.43193,
        "serviceType": "delivery",
        "ratingsOutOfFive": "true",
        "je-tgl-ops_include_closed": "true",
        "je-tgl-tmp_banners": "true"
    }
    headers = {
        "User-Agent": "Mozilla/5.0",
        "Accept": "application/json"
    }

    response = requests.get(url, params=params, headers=headers)
    data = response.json()

    print("Example record:", data["restaurants"][0])  # Debug: show sample structure

    restaurants = []
    for r in data.get("restaurants", []):
        restaurants.append({
            "name": r.get("name"),
            "rating": r.get("rating", {}).get("starRating"),
            "city": r.get("address", {}).get("city"),
            "address": r.get("address", {}).get("firstLine"),
            "eta": r.get("deliveryEtaMinutes"),
            "delivery_cost": r.get("deliveryCost"),
            "min_delivery_value": r.get("minimumDeliveryValue")
        })

    return restaurants

# Flask route
@app.route("/scrape", methods=["GET"])
def scrape():
    try:
        data = real_scrape()
        print(f"Scraped {len(data)} restaurants, saving to database...")

        # 清空旧数据并插入新数据
        collection.delete_many({})
        if data:
            collection.insert_many(data)

        # 查询并返回（不包含 _id）
        cursor = collection.find({}, {"_id": 0})
        result = list(cursor)

        json_str = json.dumps({
            "message": "Scraped from Pyszne.pl API",
            "count": len(result),
            "data": result
        }, indent=2,ensure_ascii=False)

        return Response(json_str, content_type='application/json')
    except Exception as e:
        print("Scrape failed:", str(e))
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001)
