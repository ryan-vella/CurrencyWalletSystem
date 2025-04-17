# CurrencyWalletSystem

A lightweight, extensible wallet API for managing balances in multiple currencies. Features include exchange rate conversion, strategy-based balance adjustment, API key-based authorization, and rate limiting. Built with .NET 9.

---

## ✨ Features

- Create wallets in any currency
- Adjust wallet balances using different strategies
- On-the-fly currency conversion using latest exchange rates
- Strategy pattern for extensibility
- IP-based rate limiting for abuse prevention
- Clean architecture principles

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server or any EF Core-supported provider

### Clone & Run

```bash
git clone https://github.com/ryan-vella/CurrencyWalletSystem.git
cd CurrencyWalletSystem.API
dotnet run
```

### 🔑 Authorization
All endpoints (except those marked `[AllowAnonymous]`) require an API Key.

### Set the API Key
In `appsettings.json`:

```json
{
  "ApiSettings": {
    "ApiKey": "your-secure-api-key"
  }
}
```

## 🧪 API Endpoints

| Method | Endpoint                         | Description                                      |
|--------|----------------------------------|--------------------------------------------------|
| **POST**   | `/wallets`                        | Create a new wallet                             |
| **GET**    | `/wallets/{id}/balance`           | Get wallet balance (with conversion)            |
| **PATCH**  | `/wallets/{id}/adjust`            | Adjust balance using a strategy                 |
| **GET**    | `/wallets/{id}/currency`          | Get wallet base currency                        |

### Example Usage

#### 1. Create a Wallet
```http
POST /wallets
Content-Type: application/json
x-api-key: your-secure-api-key
```
```json
{
  "currency": "USD"
}
```

### ⚙️ Strategies

```
Strategy Type	Description
AddFunds	Increases wallet balance
SubtractFunds	Decreases balance, fails if insufficient
ForceSubtractFunds	Decreases balance without checks
```
### 🛡️ Rate Limiting
10 requests per minute per IP

Status code 429 on limit exceeded

Includes Retry-After: 60 header

### 🧠 Architecture Overview
Controllers: API endpoints

Services: Core logic and orchestration

Strategies: Pluggable logic for balance changes

Filters: API key authentication

EF Core: Data persistence and querying
