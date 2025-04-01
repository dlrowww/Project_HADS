from flask import Flask, request, jsonify, Response
import requests
import json
import pymongo

app = Flask(__name__)

# MongoDB 配置
client = pymongo.MongoClient("mongodb://db:27017/")
db = client["transport"]
collection = db["flixbus_trips"]

# 城市映射
city_map = {
    "Warsaw": "40e19c59-8646-11e6-9066-549f350fcb0c",
    "Gdansk": "40de6982-8646-11e6-9066-549f350fcb0c"
}

# 核心抓取函数
def flixbus_scrape(from_city, to_city, date):
    url = "https://global.api.flixbus.com/search/service/v4/search"
    params = {
        "from_city_id": city_map.get(from_city),
        "to_city_id": city_map.get(to_city),
        "departure_date": date,
        "products": '{"adult":1}',
        "currency": "USD",
        "locale": "en_US",
        "search_by": "cities",
        "include_after_midnight_rides": 1,
        "disable_distribusion_trips": 0,
        "disable_global_trips": 0
    }
    headers = {
        "User-Agent": "Mozilla/5.0",
        "Accept": "application/json"
    }

    response = requests.get(url, params=params, headers=headers)
    data = response.json()

    trips = data.get("trips", [])
    results = []

    for t in trips:
        trip_results = t.get("results", {})
        for uid, details in trip_results.items():
            results.append({
                "uid": uid,
                "status": details.get("status"),
                "transfer_type": details.get("transfer_type"),
                "departure_date": details.get("departure", {}).get("date"),
                "arrival_date": details.get("arrival", {}).get("date"),
                "price_total": details.get("price", {}).get("total"),
                "available_seats": details.get("available", {}).get("seats"),
                "intermediate_stations_count": details.get("intermediate_stations_count")
            })

    return results

# 接口路由
@app.route("/flixbus", methods=["GET"])
def scrape_flixbus():
    from_city = request.args.get("from", "Warsaw")
    to_city = request.args.get("to", "Gdansk")
    date = request.args.get("date", "31.05.2025")

    try:
        data = flixbus_scrape(from_city, to_city, date)

        # 清除旧数据并写入新数据
        collection.delete_many({})
        if data:
            collection.insert_many(data)

        # 取出并返回（去除 _id）
        cursor = collection.find({}, {"_id": 0})
        result = list(cursor)

        return Response(
            json.dumps({
                "message": f"Trips from {from_city} to {to_city} on {date}",
                "count": len(result),
                "data": result
            }, indent=2, ensure_ascii=False),
            content_type="application/json"
        )

    except Exception as e:
        return jsonify({"error": str(e)}), 500

# 运行服务
if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001)
