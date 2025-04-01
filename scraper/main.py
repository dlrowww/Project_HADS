from flask import Flask, request, jsonify, Response
import requests
import json

app = Flask(__name__)

# 示例城市 ID 映射，可根据需要扩展
city_map = {
    "Warsaw": "40e19c59-8646-11e6-9066-549f350fcb0c",
    "Gdansk": "40de6982-8646-11e6-9066-549f350fcb0c"
}

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

    # 打印第一个示例（仅打印一次）
    if trips:
        print("示例 Trip 数据结构：")
        print(json.dumps(trips[0], indent=2, ensure_ascii=False))

    result = []
    for t in trips:
        result.append({
            "departure": t.get("departure"),
            "arrival": t.get("arrival"),
            "from": t.get("from", {}).get("name"),
            "to": t.get("to", {}).get("name"),
            "price": t.get("price", {}).get("amount")
        })

    return result

@app.route("/flixbus", methods=["GET"])
def scrape_flixbus():
    from_city = request.args.get("from", "Warsaw")
    to_city = request.args.get("to", "Gdansk")
    date = request.args.get("date", "31.05.2025")

    try:
        trips = flixbus_scrape(from_city, to_city, date)

        return Response(
            json.dumps({
                "message": f"Trips from {from_city} to {to_city} on {date}",
                "count": len(trips),
                "data": trips
            }, indent=2, ensure_ascii=False),
            content_type="application/json"
        )

    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001)