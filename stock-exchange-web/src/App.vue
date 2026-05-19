<template>
  <main class="app-wrapper">
    <header class="app-header">
      <h1>Trading <span>Terminal</span></h1>
      <div class="status-dot" :class="{ 'connected': isConnected }">
        {{ isConnected ? 'Online' : 'Offline' }}
      </div>
    </header>

    <div class="main-layout">
      <aside class="sidebar">
        <section class="order-card">
          <div class="card-header">New Order</div>
          <div class="form-body">
            <div class="input-field">
              <label>Ticker</label>
              <input :class="{ 'input-error': errors.symbol }" v-model="order.symbol" @input="order.symbol = order.symbol.toUpperCase(); clearError('symbol')" maxlength="6" placeholder="PETR4" />
              <span v-if="errors.symbol" class="error-message">{{ errors.symbol }}</span>
            </div>
            
            <div class="row">
              <div class="input-field">
                <label>Price</label>
                <input type="text" :value="order.price?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })" @input="handlePriceInput" placeholder="0,00" />
                <span v-if="errors.price" class="error-message">{{ errors.price }}</span>
              </div>
              <div class="input-field">
                <label>Amount</label>
                <input :class="clearError('amount')" v-model.number="order.amount" type="number" placeholder="100" />
                <span v-if="errors.amount" class="error-message">{{ errors.amount }}</span>
              </div>
            </div>

            <div class="button-group">
              <button @click="sendOrder('B')" class="btn btn-buy">BUY</button>
              <button @click="sendOrder('S')" class="btn btn-sell">SELL</button>
            </div>
          </div>
        </section>

        <section class="share-card">
          <div class="card-header">Your Wallet</div>
          <div class="share-list">
            <div v-for="pos in shares" :key="pos.symbol" class="share-item">
              <div class="pos-info">
                <span class="pos-symbol">{{ pos.symbol }}</span>
                <span class="pos-qty">{{ pos.totalAmount }} un.</span>
              </div>
              <div class="pos-values">
                <span class="pos-exposure">R$ {{ pos.financialExposure.toLocaleString('pt-BR', {minDigits: 2}) }}</span>
                <span class="pos-avg">Average: R$ {{ pos.averagePrice.toLocaleString('pt-BR', {minDigits: 2}) }}</span>
              </div>
            </div>
            <div v-if="shares.length === 0" class="empty-mini">No open positions</div>
          </div>
        </section>
      </aside>

      <section class="orders-section">
        <div class="card-header">Recent Orders</div>
        <div class="table-container">
          <table>
            <thead>
              <tr>
                <th>Share</th>
                <th>Operation</th>
                <th>Price</th>
                <th>Amount</th>
                <th>Operating Value</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(order, index) in ordersExecuted" :key="index" class="order-row">
                <td class="symbol-col">{{ order.symbol }}</td>
                <td :class="order.side === 'B' ? 'text-buy' : 'text-sell'">{{ order.side === 'B' ? 'BUY' : 'SELL' }}</td>
                <td class="font-mono">R$ {{ order.price.toLocaleString('pt-BR') }}</td>
                <td class="font-mono">{{ order.amount }}</td>
                <td class="font-mono">R$ {{ order.operatingValue.toLocaleString('pt-BR') }}</td>
                <td>
                  <span class="badge" :class="order.status === 'E' ? 'bg-success' : 'bg-error'">
                    {{ order.status === 'E' ? 'Executed' : 'Rejected' }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </main>
  <transition-group name="toast" tag="div" class="toast-container">
  <div v-for="toast in toasts" :key="toast.id" class="toast" :class="toast.type">
    <div class="toast-content">
      <span class="toast-icon">{{ toast.type === 'success' ? '✅' : '❌' }}</span>
      <div class="toast-text">
        <div class="toast-title">{{ toast.title }}</div>
        <div class="toast-msg">{{ toast.message }}</div>
      </div>
    </div>
  </div>
</transition-group>
</template>


<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import * as signalR from '@microsoft/signalr'

const errors = ref({})
const ordersExecuted = ref([])
const shares = ref([])
const isConnected = ref(false)
const toasts = ref([])

const order = reactive({
  symbol: '',
  price: null,
  amount: null
})

let connection = null;

async function fetchShares() {
  try {
    const response = await fetch('http://localhost:5151/api/share/all')
    if (response.ok) {
      shares.value = await response.json()
    }
  } catch (e) {
    showToast('', "Error searching for wallet.", 'error')
  }
}

onMounted(() => {
  fetchShares();
  getOrders();
  connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5151/tradingHub")
      .withAutomaticReconnect()
      .build();

  connection.off("OrderReportReceived");

  connection.on("OrderReportReceived", (report) => {
      fetchShares();

      const status = report.status === 'E' ? 'success' : 'error';
      const action = report.side === 'B' ? 'Buy' : 'Sell';
      const title = report.status === 'E' ? 'Order Executed' : 'Order Rejected';

      showToast(
        title, 
        `${action} ${report.amount} unit. from ${report.symbol} to R$ ${report.price.toLocaleString('pt-BR')}`,
        status
      );

      getOrders();
  });

  connection.start()
    .then(() => isConnected.value = true)
    .catch(err => isConnected.value = false);
});

onUnmounted(() => {
  if (connection) {
      connection.stop();
  }
});

function handlePriceInput(event) {
  clearError('price')

  let value = event.target.value.replace(/\D/g, "");

  if (value.length > 0) {
    const floatValue = parseFloat(value) / 100;
    order.price = floatValue;
    
    event.target.value = floatValue.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  } else {
    order.price = null;
    event.target.value = "";
  }
}

async function getOrders() {
  try {
    const response = await fetch('http://localhost:5151/api/order/all', {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' }
    })
    
    if (isResponseSuccess(response)) {
      ordersExecuted.value = await response.json();
    }
  } catch (error) {
    showToast("", "The requested orders could not be obtained.", 'error')
  }
}

async function sendOrder(type) {
  errors.value = {}

  if (!order.symbol || !order.price || !order.amount) {
    showToast("", "Fill in all the fields.", 'error')
    return
  }

  const payload = {
    symbol: order.symbol.toUpperCase(),
    price: order.price,
    amount: order.amount,
    side: type
  }

  try {
    const response = await fetch('http://localhost:5151/api/order/new', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })

    if (isResponseSuccess(response)) {
      showToast("", "Order sent successfully!");
      resetForm();
      return;
    }
    else {
      const errorData = await response.json();
      
      showToast("", errorData.details, 'error');

      if (errorData.fields) {
        errorData.fields.forEach(field => {
          errors.value[field.name] = field.message;
        });
      }
    }
  } catch (error) {
    showToast("", "We were unable to send your order.", 'error')
  }
}

function isResponseSuccess(response) {
  return response.status >= 200 && response.status <= 299
}

function showToast(title, message, type = 'success') {
  const id = Date.now();
  toasts.value.push({ id, title, message, type });
  
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id);
  }, 4000);
}

function clearError(fieldName) {
  if (errors.value[fieldName]) {
    delete errors.value[fieldName];
  }
}

function resetForm(){
  errors.value = {}
  order.symbol = ''
  order.price = null
  order.amount = null
}
</script>

<style scoped>
.app-wrapper {
  font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: #333;
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
  background-color: #fff;
}
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px solid #eee;
  margin-bottom: 30px;
  padding-bottom: 10px;
}

.app-header h1 { font-size: 24px; color: #2c3e50; margin: 0; }

.status-dot { font-size: 13px; font-weight: bold; color: #999; }
.status-dot.connected { color: #28a745; }

.main-layout {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 40px;
}

.order-card {
  background: #fdfdfd;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.card-header {
  font-weight: bold;
  margin-bottom: 20px;
  color: #555;
  text-transform: uppercase;
  font-size: 14px;
}

.input-field { margin-bottom: 15px; }
.input-field label { display: block; font-size: 12px; margin-bottom: 5px; color: #666; }
.input-field input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  box-sizing: border-box;
  font-size: 16px;
}
.button-group { display: flex; gap: 10px; margin-top: 20px; }

.btn {
  flex: 1;
  padding: 15px;
  border: none;
  border-radius: 4px;
  font-weight: bold;
  font-size: 14px;
  cursor: pointer;
  color: white;
  transition: opacity 0.2s;
}

.btn-buy { background-color: #28a745; }
.btn-sell { background-color: #dc3545; }
.btn:hover { opacity: 0.85; }
.orders-section h3 { margin-top: 0; color: #2c3e50; font-size: 18px; }

table {
  width: 100%;
  border-collapse: collapse;
}

th {
  text-align: left;
  padding: 12px;
  background-color: #f8f9fa;
  border-bottom: 2px solid #dee2e6;
  color: #495057;
  font-size: 13px;
}

td {
  padding: 12px;
  border-bottom: 1px solid #eee;
  font-size: 14px;
}

.text-buy { color: #28a745; font-weight: bold; }
.text-sell { color: #dc3545; font-weight: bold; }

.badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: bold;
  text-transform: uppercase;
}

.bg-success { background-color: #e6f4ea; color: #1e7e34; }
.bg-error { background-color: #fce8e6; color: #c53030; }

.empty-msg { text-align: center; color: #aaa; padding: 40px; }

.input-field input {
  text-align: center;
  font-family: 'JetBrains Mono', monospace;
}

input::placeholder {
  text-align: center;
}

.error-message {
  color: #dc3545;
  font-size: 11px;
  margin-top: 4px;
  display: block;
  text-align: center;
}

.input-error {
  border-color: #dc3545 !important;
}

.error-message {
  color: #dc3545;
  font-size: 11px;
  margin-top: 4px;
  display: block;
  text-align: center;
  transition: all 0.2s ease;
}

.main-layout {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 20px;
}

.share-card {
  margin-top: 20px;
  background: #2c3e50;
  color: white;
  border-radius: 8px;
  padding: 15px;
}

.share-card .card-header { color: #ecf0f1; border-bottom: 1px solid #34495e; padding-bottom: 10px; }

.share-item {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid #34495e;
}

.pos-symbol { font-weight: bold; color: #3498db; display: block; }
.pos-qty { font-size: 12px; color: #bdc3c7; }
.pos-values { text-align: right; }
.pos-exposure { display: block; font-weight: bold; color: #2ecc71; }
.pos-avg { font-size: 10px; color: #bdc3c7; }

.empty-mini { font-size: 12px; color: #7f8c8d; text-align: center; padding: 20px; }

.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.toast {
  min-width: 280px;
  padding: 16px;
  border-radius: 8px;
  background: white;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  display: flex;
  align-items: center;
  border-left: 6px solid #ccc;
}

.toast.success { border-left-color: #28a745; }
.toast.error { border-left-color: #dc3545; }

.toast-content { display: flex; align-items: center; gap: 12px; }
.toast-icon { font-size: 20px; }
.toast-title { font-weight: bold; font-size: 14px; margin-bottom: 2px; }
.toast-msg { font-size: 12px; color: #666; }
.toast-enter-active, .toast-leave-active {
  transition: all 0.4s ease;
}
.toast-enter-from {
  transform: translateX(100%);
  opacity: 0;
}
.toast-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

</style>