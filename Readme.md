# TechChallenge Fase 2 - Orquestração

Repositório orquestrador do Tech Challenge Fase 2. Ele reúne as APIs como **git submodules** e o `docker-compose.yml` para subir tudo junto. O código de cada API vive no repositório dela; aqui ficam apenas os ponteiros (commits) e a orquestração.

## APIs (submodules)

| API | Pasta | Repositório | Branch |
|---|---|---|---|
| CatalogAPI | `services/catalog-api` | `TechChallenge_Fase2_CatalogAPI` | `main` |
| NotificationAPI | `services/notification-api` | `TechChallenge_Fase2_NotificationAPI` | `main` |
| UsersAPI | `services/users-api` | `TechChallenge_Fase2_UsersAPI` | `master` |

Todos na organização `Arquitetura-Sistemas-Grupo-3`.

## Pré-requisitos

- Git
- Chave SSH cadastrada no GitHub e autorizada na organização (SSO)
- Acesso de leitura aos quatro repositórios
- Docker e Docker Compose

## Clonar o projeto (com as APIs)

```bash
git clone --recurse-submodules -b main git@github.com:Arquitetura-Sistemas-Grupo-3/TechChallenge_Fase2_Orquestracao.git
cd TechChallenge_Fase2_Orquestracao
```

Se você já clonou sem `--recurse-submodules` (as pastas em `services/` ficaram vazias):

```bash
git submodule update --init --recursive
```

Conferir:

```bash
git submodule status
```

As três APIs devem aparecer sem `-` na frente.

### Erro `Permission denied (publickey)` nos submodules

O clone principal pede a passphrase da chave, mas os clones dos submodules rodam em processos separados e precisam da chave carregada no `ssh-agent`. No Git Bash:

```bash
eval "$(ssh-agent -s)"
ssh-add ~/.ssh/id_ed25519
git submodule update --init --recursive
```

Teste a autenticação com `ssh -T git@github.com`.

## Atualizar as APIs para rodar o compose

Os devs trabalham nos repositórios das próprias APIs. Quem for usar o orquestrador **não precisa registrar nada**: basta puxar o último commit da branch de cada API (`main` para Catalog e Notification, `master` para Users, conforme o `.gitmodules`).

```bash
git pull
git submodule update --init --remote
```

Depois, suba os serviços:

```bash
docker compose up --build
```

### Atalho: alias `sync` (uma vez por clone)

```bash
git config alias.sync '!git pull && git submodule update --init --remote'
```

A partir daí, para deixar tudo atualizado:

```bash
git sync
```

### Observações

- O `git status` vai mostrar `services/...` como modificado depois do update. É esperado: as pastas estão à frente do commit registrado. **Não é preciso commitar.**
- **Não** configure `submodule.recurse true` neste fluxo: o `git pull` voltaria cada API para o commit registrado, desfazendo a atualização. Se já configurou, desfaça com `git config --unset submodule.recurse`.
- `git pull` sozinho atualiza apenas o orquestrador (compose, `.env.example`, README). As APIs só avançam com o `git submodule update --remote`.
- Se uma API estiver com alterações locais não commitadas, o update pode falhar. Descarte ou faça commit antes.

## Alterando o código de uma API

Os submodules ficam em *detached HEAD* após o clone. Antes de editar, entre na pasta e troque para a branch da API:

```bash
cd services/catalog-api
git checkout main        # users-api usa master
```

Faça commit e push **dentro do repositório da API**:

```bash
git add .
git commit -m "sua mensagem"
git push origin main
```

Não é necessário mexer no orquestrador: quem for rodar o compose recebe a mudança com `git sync`.

## Subindo os serviços

Crie o arquivo `.env` na raiz (ele não é versionado, não faça commit dele) e suba:

```bash
docker compose up --build
```

## Problemas comuns

| Sintoma | Causa / solução |
|---|---|
| Pastas em `services/` vazias | Clonou sem `--recurse-submodules`. Rode `git submodule update --init --recursive`. |
| `Permission denied (publickey)` | Chave não está no `ssh-agent`. Veja a seção acima. |
| `no submodule mapping found in .gitmodules` | Falta a entrada do submodule no `.gitmodules`. |
| `git submodule update --remote` falha na UsersAPI | A branch dela é `master`, confira o `branch` no `.gitmodules`. |
| Aviso `LF will be replaced by CRLF` | Apenas aviso do Git no Windows, pode ignorar. |