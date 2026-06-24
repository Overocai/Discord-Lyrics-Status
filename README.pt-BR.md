# 🎵 Discord Lyrics Status

> [English](README.md) · **Português 🇧🇷**

Transforme o seu **status personalizado do Discord** em uma exibição ao vivo,
linha por linha, da letra do que você está ouvindo — com um app de desktop
bonito que mostra a **capa do álbum**, a música, uma **barra de progresso** e a
letra passando em tempo real.

Ele lê a música direto do "tocando agora" do Windows (aquele overlay que aparece
com as teclas de volume), então funciona com **Spotify, YouTube, navegadores e
mais — sem precisar de API do Spotify nem chaves de desenvolvedor.**

---

## 🖥️ A interface

A janela (feita com `customtkinter`) é um card estilo "tocando agora" do
Spotify:

- 🖼️ **Capa do álbum** — puxada direto do Windows, com cantos arredondados.
- 🎵 **Título e artista** da música atual.
- 📊 **Barra de progresso** com o tempo decorrido e a duração total.
- 🎤 **Letra sincronizada** no centro — a **linha atual fica destacada em
  ciano**, com a anterior e a próxima esmaecidas em cima e embaixo.
- 🟢 **Rodapé** mostrando o texto exato que está no seu status do Discord e em
  qual conta você está conectado.

Tudo atualiza sozinho conforme a música toca, e o seu status do Discord
acompanha linha por linha.

> Prefere terminal? Rode `python main.py --cli` pra um painel de texto com `rich`.

---

## ✨ Recursos

- **Letra ao vivo → status do Discord**, linha por linha.
- **Interface moderna** com capa do álbum, barra de progresso e linha atual em
  destaque.
- **Funciona com qualquer player** que apareça nos controles de mídia do Windows
  (Spotify, YouTube no navegador, Groove, etc.) — **sem API do Spotify**.
- **Tratamento esperto do YouTube** — remove lixos como `(Official Video)`,
  `(Clipe Oficial)`, `(Lyrics)`, `[HD]`, `- Topic`, `VEVO` antes de buscar.
- **Sincronia precisa** — lida com vários timestamps por linha do LRC usando
  busca binária.
- **Letras em cache no disco**, então cada música só é buscada uma vez.
- **Respeita rate limit** — trata as respostas `429` do Discord.
- **Token fora do código** — carregado do `config.json` (ignorado pelo git) ou da
  variável `DISCORD_TOKEN`, então dá pra publicar o repositório com segurança.

---

## ⚠️ Aviso

Esta ferramenta automatiza uma **conta de usuário** (um "selfbot"), o que é
**contra os Termos de Serviço do Discord**. Ela só altera o *seu próprio* status
e nunca mexe em servidores ou outros usuários, mas, em tese, usar isso pode levar
a uma punição na conta. **Use por sua conta e risco, numa conta que você esteja
disposto a perder.** Este projeto é para fins educacionais.

---

## 📦 Requisitos

- **Windows 10 / 11** (usa a API de controle de mídia do Windows)
- **Python 3.9+**
- Um **token de usuário** do Discord

## 🚀 Instalação

```bash
git clone https://github.com/Overocai/Discord-Lyrics-Status.git
cd Discord-Lyrics-Status
```

**Instale as dependências.** No Windows, o jeito mais fácil é só dar **dois
cliques no `install.bat`** — ele roda o `pip install -r requirements.txt` pra
você e dá uma pausa no final pra você ler o resultado. Prefere o terminal? Faça
na mão:

```bash
pip install -r requirements.txt
```

Depois crie o seu config privado a partir do modelo:

```bash
copy config.example.json config.json   # Windows
```

Abra o `config.json` e cole o seu token em `"token"`, **ou** use uma variável de
ambiente (recomendado):

```powershell
$env:DISCORD_TOKEN = "seu_token_aqui"
```

Depois rode:

```bash
python main.py            # interface gráfica (padrão)
python main.py --cli      # painel no terminal
```

Toque uma música e a janela ganha vida — o seu status do Discord atualiza
sozinho. Feche a janela (ou aperte Ctrl+C no modo CLI) pra limpar o status.

## ⚙️ Configuração (`config.json`)

| Chave | Padrão | Descrição |
|-------|--------|-----------|
| `token` | `""` | Token de usuário do Discord (a variável `DISCORD_TOKEN` tem prioridade). |
| `status_prefix` | `"🎵 "` | Texto antes de cada linha da letra. |
| `emoji_name` | `""` | Emoji opcional ao lado do status. |
| `poll_interval` | `0.3` | Segundos entre as checagens de mídia. |
| `line_lead` | `0.4` | Mostra cada linha um pouco antes pra compensar latência. |
| `max_status_length` | `128` | Limite do Discord pro texto do status. |
| `show_song_when_no_lyrics` | `true` | Cai pra `Título — Artista` se não achar letra. |
| `clear_on_pause` | `true` | Limpa o status quando pausa/para. |
| `cache_lyrics` | `true` | Guarda as letras em `.cache/`. |
| `synced_only` | `true` | Aceita só letras sincronizadas (`.lrc`). |
| `providers` | `[]` | Restringe o `syncedlyrics` a provedores específicos. |

## 🧠 Como funciona

```
API de mídia do Windows ──> media.py   (música, posição, duração, capa)
syncedlyrics            ──> lyrics.py   (baixa + cacheia + parseia LRC, acha a linha)
                             worker.py   (loop em segundo plano, atualiza o estado)
API REST do Discord     <── discord_client.py  (define o status, com rate limit)
customtkinter           <── gui.py       (janela)  |  rich -> app.py (CLI)
```

## 🔑 Pegando o seu token

O jeito mais rápido é deixar o próprio Discord te entregar o **seu** token direto
pelo **Console** do navegador:

1. Abra **<https://discord.com/app>** no navegador (Chrome, Edge ou Firefox) e
   entre na conta que você quer usar.
2. Aperte **F12** (ou `Ctrl`+`Shift`+`I`) pra abrir o DevTools e clique na aba
   **Console**.
3. O Chrome/Edge bloqueiam colar no console na primeira vez — se aparecer um aviso
   de "Self-XSS" / "Não cole código aqui", digite **`allow pasting`** e aperte
   Enter.
4. Cole esta **única linha** e aperte **Enter**:

   ```js
   window.webpackChunkdiscord_app.push([[Symbol()],{},o=>{for(let e of Object.values(o.c))try{if(!e.exports||e.exports===window)continue;e.exports?.getToken&&(token=e.exports.getToken());for(let o in e.exports)e.exports?.[o]?.getToken&&"IntlMessagesProxy"!==e.exports[o][Symbol.toStringTag]&&(token=e.exports[o].getToken())}catch{}}]),window.webpackChunkdiscord_app.pop(),token;
   ```

5. O console mostra o seu token como um texto entre aspas, ex.:
   `'MTI4ODgz...XYZ'`. Copie o que está **dentro das aspas** — esse é o seu token.
6. Cole no `config.json` em `"token"`, ou defina a variável de ambiente
   `DISCORD_TOKEN`.

> 💡 O código só lê o token da **sua própria** sessão logada e mostra ele
> localmente — não envia nada pra lugar nenhum.

<details>
<summary>Alternativa: a aba Network</summary>

Abra o Discord, aperte **F12 → Network**, faça qualquer ação (ex.: enviar uma
mensagem), clique numa requisição e copie o valor do cabeçalho **`authorization`**.
</details>

⚠️ **Nunca compartilhe o seu token** — quem tiver ele tem acesso total à sua
conta. O `.gitignore` deste repo já exclui o `config.json` pra evitar vazamento
acidental.

## 🛠️ Resolução de problemas

- **`Invalid Discord token (401)`** — token errado/expirado; pegue um novo.
- **Sem letra** — a música pode não ter letra sincronizada (cai no título).
  Apague a pasta `.cache/` pra forçar uma nova busca.
- **Música errada detectada** — se Spotify *e* uma aba do YouTube tocam ao mesmo
  tempo, o Windows reporta a sessão mais recente. Pause a que você não quer.
- **Letra um pouco fora de sincronia** — ajuste o `line_lead` no `config.json`.

## 👤 Autor

Feito por **overocai** — [Discord](https://discord.com/users/1288832011452153910) (`1288832011452153910`).

## 📄 Licença

MIT © overocai — veja [LICENSE](LICENSE).
