/*************************************************************************
 * main.js  —  通用脚本
 * - 所有页面：注入左侧导航栏并高亮当前页
 * - 仅在 search.html（含 #origin 等控件）时，初始化搜索逻辑
 *************************************************************************/

/* ---------- 1. 通用：注入导航栏并高亮 ---------- */
function injectSidebar() {
  fetch('navbar.html')
    .then(res => res.text())
    .then(html => {
      // 若页面没有占位符就主动创建
      let holder = document.getElementById('navbar');
      if (!holder) {
        holder = document.createElement('div');
        holder.id = 'navbar';
        document.body.prepend(holder);
      }
      holder.outerHTML = html;

      // 高亮当前链接
      const current = location.pathname.split('/').pop() || 'index.html';
      document.querySelectorAll('.navmenu a').forEach(a => {
        if (a.getAttribute('href') === current) a.classList.add('active');
      });
    })
    .catch(console.error);
}
injectSidebar();

/* ---------- 2. 仅当存在搜索控件时才执行以下代码 ---------- */
const originSel = document.getElementById('origin');
const destSel   = document.getElementById('destination');
const searchBtn = document.getElementById('searchBtn');

if (originSel && destSel && searchBtn) {
  /* ——— 2.1 初始化城市下拉 ——— */
  const cities = ["Gdańsk","Warsaw","Katowice","Wrocław","Łódź",
                  "Kraków","Poznań","Szczecin","Lublin","Białystok"];
  cities.forEach(c => {
    originSel.insertAdjacentHTML('beforeend', `<option value="${c}">${c}</option>`);
    destSel.insertAdjacentHTML('beforeend',   `<option value="${c}">${c}</option>`);
  });
  originSel.value = cities[0];
  destSel.value   = cities[1];

  /* ——— 2.2 初始化日期选择器 ——— */
  const dateInput = document.getElementById('date');
  const today     = new Date();
  flatpickr(dateInput, {
    dateFormat: 'Y-m-d',
    defaultDate: today,
    minDate: today,
    maxDate: new Date().fp_incr(6),
    showMonths: 2,
    allowInput: false
  });

  /* ——— 2.3 绑定搜索按钮 ——— */
  searchBtn.addEventListener('click', async () => {
    document.getElementById('msg').textContent = '';
    const resultsDiv = document.getElementById('results');
    resultsDiv.innerHTML = '<h2 id="resultsTitle">Results:</h2>';
    document.getElementById('resultsTitle').style.display = 'block';

    const qs = new URLSearchParams({
      origin:      originSel.value,
      destination: destSel.value,
      date:        dateInput.value,
      transport:   'bus',
      people:      document.getElementById('people').value
    });

    let resp;
    try {
      resp = await fetch(`/api/offers/search?${qs.toString()}`);
    } catch {
      document.getElementById('msg').textContent = 'Network error';
      return;
    }

    if (!resp.ok) {
      document.getElementById('msg').textContent = `Error ${resp.status}`;
      return;
    }

    const data = await resp.json();
    if (!Array.isArray(data) || data.length === 0) {
      resultsDiv.innerHTML = '<p>No offers found.</p>';
      return;
    }

    data.forEach(o => {
      resultsDiv.insertAdjacentHTML('beforeend', `
        <div class="card">
          <div class="card-left">
            <div class="time-row">
              <span class="bold">${o.departureTime}</span>
              <div class="time-line">
                <div class="date-on-line">${dateInput.value}</div>
              </div>
              <span class="bold">${o.arrivalTime}</span>
              <div class="duration-on-line">
                ${o.durationHours}:${o.durationMinutes.toString().padStart(2,'0')} hrs
              </div>
            </div>
            <div class="city-row">
              <span>${o.fromCity}</span><span>${o.toCity}</span>
            </div>
            <div class="id-badge">
              <strong>BUS</strong><span class="divider"></span><span>${o.shortId}</span>
            </div>
          </div>
          <div class="card-right">
            <div class="price">${o.price} ${o.currency}</div>
            <div class="seats">Seats: ${o.availableSeats ?? ''}</div>
            <button class="continue-btn">Continue</button>
          </div>
        </div>
      `);
    });
  });
}
/* ---------- 3. 其它页面什么都不做 ---------- */
