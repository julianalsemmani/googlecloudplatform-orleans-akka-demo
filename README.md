# Orleans vs Akka Demo – Master’s Thesis Project

This project compares **Microsoft Orleans** and **Akka.NET** for actor-based distributed systems, using a shopping cart scenario inspired by [Google Cloud Platform’s Online Boutique](https://github.com/GoogleCloudPlatform/microservices-demo). It is developed as part of a master’s thesis in Informatics.

## Structure

- `AkkaShopDemo/` – Shopping cart service implemented using **Akka.NET**
- `OrleansShopDemo/` – Shopping cart service using **Microsoft Orleans**
- `ShopFrontend/` – Frontend written in **Go** (inspired by Online Boutique's frontend)
- `ShopLoadGenerator/` – Load generator written in **Python** for benchmarking and stress-testing

## Run Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Go](https://golang.org/)
- [Python 3.x](https://www.python.org/)
- [Docker](https://www.docker.com/) (for running Orleans MembershipTable with Redis or SQL)

### Build & Run

**Akka.NET service**
```bash
cd AkkaShopDemo
dotnet build
dotnet run
```

**Orleans service**
```bash
cd OrleansShopDemo
dotnet build
dotnet run
```

**Frontend**
```bash
cd ShopFrontend
go run main.go
```

**Load generator**
```bash
cd ShopLoadGenerator
pip install -r requirements.txt
python main.py
```

**Benchmarking**
Use **ShopLoadGenerator** to simulate realistic traffic using Locust. You can also integrate tools like k6 externally to collect performance metrics.

## Notes
> **Note**  
> This project is **not** hosted on Google Cloud — it is architecturally inspired by GCP’s Online Boutique demo.

> **Important**  
> The Orleans service requires a **MembershipTable** backend, such as **Redis** or **SQL Server**, for clustering.


> **Configuration**  
> This project uses **SQL Server** as the MembershipTable backend for Orleans.  
> Configuration details can be found in `appsettings.json` within the `OrleansShopDemo` project.

> **Schema**  
> The required Orleans SQL tables are listed [here](https://github.com/julianalsemmani/googlecloudplatform-orleans-akka-demo/blob/main/Db.txt).



