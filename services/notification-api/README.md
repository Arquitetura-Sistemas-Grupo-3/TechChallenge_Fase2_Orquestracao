# NotificationsAPI

Microsserviço de notificações da plataforma **FIAP Cloud Games (FCG)** — Tech Challenge Fase 2 (PosTech .NET).

Responsável por consumir eventos do barramento de mensageria (RabbitMQ) e "enviar" (simular via log) e-mails transacionais:

- **E-mail de boas-vindas** ao consumir `UserCreatedEvent` (publicado pelo `UsersAPI`).
- **E-mail de confirmação de compra** ao consumir `PaymentProcessedEvent` com status `Approved` (publicado pelo `PaymentsAPI`).

Este serviço **não publica** eventos e **não persiste** dados — apenas consome e loga.

## Stack

- .NET 10 (ASP.NET Core)
- MassTransit 8 + RabbitMQ
- Docker (multi-stage build)
- Kubernetes (Deployment + Service + ConfigMap + Secret)

## Estrutura

```
NotificationsAPI/
├── NotificationsAPI/              # Projeto .NET
│   ├── Contracts/Events/          # Contratos dos eventos consumidos
│   ├── Consumers/                 # Handlers dos eventos
│   ├── Program.cs                 # Bootstrap + configuração do MassTransit
│   └── appsettings.json
├── k8s/                           # Manifestos Kubernetes
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── deployment.yaml
│   └── service.yaml
├── Dockerfile                     # Multi-stage build
├── docker-compose.yml             # Ambiente local (RabbitMQ + API)
└── README.md
```

## Variáveis de ambiente

| Variável | Descrição | Default | Origem no K8s |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Production` | ConfigMap |
| `RabbitMq__Host` | Hostname do broker RabbitMQ | `localhost` | ConfigMap |
| `RabbitMq__Port` | Porta AMQP do broker | `5672` | ConfigMap |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ | `/` | ConfigMap |
| `RabbitMq__Username` | Usuário de conexão | `guest` | **Secret** |
| `RabbitMq__Password` | Senha de conexão | `guest` | **Secret** |

> No .NET, o duplo underscore `__` é o separador de seções (equivale a `RabbitMq:Host` em `appsettings.json`).

## Executando localmente (Docker Compose)

Sobe RabbitMQ + a API já configurada para se conectar nele:

```bash
docker-compose up --build
```

- Painel de gerenciamento do RabbitMQ: http://localhost:15672 (guest/guest)
- API: http://localhost:8080
- Health check: http://localhost:8080/health

Para testar o consumo dos eventos você pode publicar mensagens manualmente pelo painel do RabbitMQ nas exchanges `Contracts.Events:UserCreatedEvent` e `Contracts.Events:PaymentProcessedEvent` (nomes gerados pelo MassTransit a partir do FullName do tipo).

## Deploy no Kubernetes

Pré-requisitos:

- Cluster local ativo (Docker Desktop Kubernetes, Minikube, Kind ou k3d).
- Imagem Docker construída e disponível no cluster.
- RabbitMQ rodando no cluster com Service chamado `rabbitmq` (ver [Repositório de Orquestração](#)).

### 1. Build da imagem

Se estiver usando **Docker Desktop Kubernetes**, o build local já fica disponível no cluster:

```bash
docker build -t notifications-api:latest .
```

Em **Minikube**, aponte o Docker para o daemon do cluster antes do build:

```bash
eval $(minikube docker-env)   # PowerShell: minikube docker-env | Invoke-Expression
docker build -t notifications-api:latest .
```

Em **Kind**, carregue a imagem no cluster:

```bash
kind load docker-image notifications-api:latest
```

### 2. Aplicar os manifestos

```bash
kubectl apply -f k8s/
```

### 3. Verificar

```bash
kubectl get pods -l app=notifications-api
kubectl logs -l app=notifications-api -f
```

O Service `notifications-api` fica acessível no cluster em `http://notifications-api:80`.

## Contratos de eventos consumidos

### UserCreatedEvent

```csharp
public record UserCreatedEvent(
    Guid UserId,
    string Name,
    string Email,
    DateTime CreatedAt);
```

### PaymentProcessedEvent

```csharp
public record PaymentProcessedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    string GameName,
    string UserEmail,
    decimal Price,
    PaymentStatus Status,   // Approved | Rejected
    DateTime ProcessedAt);
```

> **Importante — alinhamento com o grupo:** o MassTransit roteia mensagens usando o `FullName` do tipo (namespace + classe). O `UsersAPI` e o `PaymentsAPI` **precisam publicar eventos no mesmo namespace `Contracts.Events`** para que este serviço consiga consumi-los. O ideal é extrair essa pasta `Contracts/Events/` para um pacote NuGet compartilhado ou copiar os mesmos arquivos nos outros repositórios.

## Filas criadas

- `notifications-user-created` — vinculada à exchange `Contracts.Events:UserCreatedEvent`
- `notifications-payment-processed` — vinculada à exchange `Contracts.Events:PaymentProcessedEvent`

Cada endpoint aplica retry automático (3 tentativas com intervalo de 5s) em caso de falha.
