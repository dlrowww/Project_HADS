from flask import Flask, jsonify
import pymongo
import requests

app = Flask(__name__)

# Connect to MongoDB
client = pymongo.MongoClient("mongodb://db:27017/")
db = client["food"]
collection = db["restaurants"]

# Actual scraping function: Call the API
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

    print("Example record:", data["restaurants"][0])

    restaurants = []
    for r in data.get("restaurants", []):
        name = r.get("name")
        rating = r.get("rating", {}).get("starRating")
        restaurants.append({
            "name": name,
            "rating": rating
        })
    return restaurants

# Flask route
@app.route("/scrape", methods=["GET"])
def scrape():
    try:
        data = real_scrape()
        print(f"Fetched {len(data)} records, saving to database...")

        for item in data:
            print(f"[Scrape] {item['name']} ***** {item['rating']}")

        # Clear old records before inserting new ones
        collection.delete_many({})
        if data:
            collection.insert_many(data)

        cursor = collection.find({}, {"_id": 0})
        data = list(cursor)

        return jsonify({
            "message": "Scraped from Pyszne.pl API",
            "count": len(data),
            "data": data
        })
    except Exception as e:
        print("Scraping failed:", str(e))
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001)
