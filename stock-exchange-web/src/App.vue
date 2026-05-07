<template>
  <main class="app-wrapper">
    <header class="app-header">
      <h1>Trading<span>Terminal</span></h1>
      <div class="status-dot" :class="{ 'connected': isConnected }">
        {{ isConnected ? 'Online' : 'Offline' }}
      </div>
    </header>

    <div class="main-layout">
      <section class="order-card">
        <div class="card-header">Nova Ordem</div>
        <div class="form-body">
          <div class="input-field">
            <label>Ativo</label>
            <input v-model="order.symbol" @input="order.symbol = order.symbol.toUpperCase()" placeholder="PETR4" />
          </div>
          
          <div class="row">
            <div class="input-field">
              <label>Preço</label>
              <input 
                type="text"
                :value="order.price?.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })"
                @input="handlePriceInput"
                placeholder="0,00"
                inputmode="decimal"
              />
            </div>
            <div class="input-field">
              <label>Quantidade</label>
              <input v-model.number="order.amount" type="number" placeholder="100" />
            </div>
          </div>

          <div class="button-group">
            <button @click="sendOrder('B')" class="btn btn-buy">COMPRAR</button>
            <button @click="sendOrder('S')" class="btn btn-sell">VENDER</button>
          </div>
        </div>
      </section>

      <section class="orders-section">
        <div class="card-header">Ordens Recentes</div>
        <div class="table-container">
          <table>
            <thead>
              <tr>
                <th>Ativo</th>
                <th>Operação</th>
                <th>Preço</th>
                <th>Qtd</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(ordem, index) in ordensExecutadas" :key="index" class="order-row">
                <td class="symbol-col">{{ ordem.symbol }}</td>
                <td :class="ordem.side === 'B' ? 'text-buy' : 'text-sell'">
                  {{ ordem.side === 'B' ? 'COMPRA' : 'VENDA' }}
                </td>
                <td class="font-mono">R$ {{ ordem.price.toLocaleString('pt-BR', {minDigits: 2}) }}</td>
                <td class="font-mono">{{ ordem.amount }}</td>
                <td>
                  <span class="badge" :class="ordem.status === 'E' ? 'bg-success' : 'bg-error'">
                    {{ ordem.status === 'E' ? 'Executada' : 'Rejeitada' }}
                  </span>
                </td>
              </tr>
              <tr v-if="ordensExecutadas.length === 0">
                <td colspan="5" class="empty-msg">Aguardando mensagens do mercado...</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </main>
</template>


<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import * as signalR from '@microsoft/signalr'

const ordensExecutadas = ref([])
const isConnected = ref(false)

const order = reactive({
  symbol: '',
  price: null,
  amount: null
})

let connection = null;

onMounted(() => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5151/tradingHub")
        .withAutomaticReconnect()
        .build();

    connection.off("OrderReportReceived");

    connection.on("OrderReportReceived", (report) => {
        ordensExecutadas.value.unshift(report);
    });

    connection.start()
      .then(() => {
        isConnected.value = true;
      })
      .catch(err => {
        isConnected.value = false;
      });
});

onUnmounted(() => {
    if (connection) {
        connection.stop();
    }
});

function handlePriceInput(event) {
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

async function sendOrder(type) {
  if (!order.symbol || !order.price || !order.amount) {
    alert("Preencha todos os campos!")
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

    if (response.status === 201 || response.status === 204) {
      alert("Ordem enviada com sucesso!");
      resetForm();
      return;
    }
    else {
      alert("Erro: " + result.error)
    }
  } catch (error) {
    alert("Não foi possível enviar a sua ordem.")
  }
}
function resetForm(){
  order.symbol = '';
  order.price = null;
  order.amount = null;
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

</style>