<div align="center">
🌿 Rotten Roots
 
**A floresta não morreu. Ela está reagindo.**
 
Survival horror 2D com body horror biológico, em que a própria floresta pune a poluição que você causa.
 
![Status](https://img.shields.io/badge/Status-Em_desenvolvimento-F75C7E?style=for-the-badge)
![Unity](https://img.shields.io/badge/Unity-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white)
 
<!-- Troque pela captura de tela principal (ex.: docs/capa.png) -->
<!-- <img src="docs/capa.png" alt="Captura de tela de Rotten Roots" width="600" /> -->
 
</div>
---
 
## 📖 Sobre o jogo
 
A **Poison Tech Corp.**, fabricante de agrotóxicos, despejou lixo químico em uma reserva natural. A natureza não morreu: sofreu uma mutação bizarra e se transformou em um **super-organismo agressivo**. A floresta passou a reagir à presença humana como um corpo combatendo uma infecção.
 
Você é um **jornalista** enviado para coletar dados e tirar fotos **antes que queimem tudo**. Para publicar a matéria sobre a contaminação, precisa reunir evidências, e para isso precisa sobreviver a uma floresta que **sente cada ação poluidora que você comete**.
 
> Projeto Final A3 (2026.2) · Tema: **Valores nos jogos, Consciência Ambiental e Sustentabilidade**
 
## 🎮 Mecânicas principais
 
### 🌳 Ira da floresta (pegada ecológica)
Tudo o que o jogador faz de "errado" estressa o ambiente. Quanto maior a ira, mais ameaças aparecem e mais passagens são bloqueadas.
 
| Ações que **aumentam** a ira | Ações que **reduzem** a ira |
|---|---|
| Disparar armas de fogo (cada tiro tem uma chance de aumentar a ira) | Usar **kits de descontaminação** biodegradáveis |
| Matar animais | Andar devagar e agachado |
| Descartar itens do inventário | Apagar a lanterna |
| Destruir arbustos e inimigos com faca | Jogar neutralizadores ecológicos no solo |
 
Os inimigos não caçam só por visão ou som, mas também pelo **rastro tóxico** que o jogador deixa.
 
### 📷 Câmera e evidências
O jogador começa com **uma câmera e uma lanterna**. É preciso fotografar inimigos, cadáveres, amostras e vítimas para juntar evidências. O rolo da câmera é limitado.
 
### 🎒 Inventário
Inventário com **8 slots** (armas, munição, kits de descontaminação, lanterna e outros itens). A munição da pistola é um contador no próprio slot da arma.
 
### 🔦 Lanterna de dinamo
Quando selecionada no inventário, a lanterna fica ligada por um tempo e gasta bateria. Depois começa a piscar e desliga, obrigando o jogador a "recarregá-la".
 
### ❤️ Vida
A vida é representada por um **coração humano anatomicamente preciso** que vai se esvaziando conforme o jogador sofre dano.
 
### 👹 Inimigos
Mutações de plantas e animais, como raízes em ossos e fungos predatórios.
 
| Inimigo | Comportamento |
|---|---|
| 🦇 Morcego | Aérea. Empurra o jogador para outra sala |
| 🐆 Onça | Terrestre. Ataca quando o jogador está por perto |
| 🐒 Macaco (neutro) | Terrestre. Só ataca quando provocado (tiro na direção dele ou aproximação excessiva) e atira pedras |
| 🌲 Árvores sinistras | Mudam de aspecto conforme a ira da floresta aumenta |
 
## 🗺️ Level design
 
- Mapa dividido em **áreas quadradas (chunks)**, inspiradas nos Zeldas clássicos.
- Sensação de **labirinto**, com **backtracking** (ida e volta por áreas já percorridas) como parte da progressão.
- Cadáveres de funcionários da empresa seguram pranchetas com anotações, que servem como **pistas e evidências**.
- Os chunks ficam todos na cena e são ativados e desativados conforme o jogador se move.
<!-- Adicione o protótipo do mapa: ![Protótipo do mapa](docs/prototipo-mapa.png) -->
 
## ⌨️ Controles
 
*(Conforme o GDD. Confirme na versão atual.)*
 
| Ação | Teclado | Controle |
|---|---|---|
| Mover | `W` `A` `S` `D` ou setas | Analógico esquerdo ou direcionais |
| Coletar item | `E` | `Y` |
| Trocar item | `1` a `8` ou clique no slot | `LB` / `RB` |
| Disparar / usar item | `Espaço` | `A` |
| Recarregar | `R` | `X` |
| Dropar item | `G` | `B` |
 
Movimento em 4 direções (diagonais bloqueadas). A direção do tiro é a última direção em que o jogador olhou.
 
## 🔬 Pesquisa (artigo científico)
 
O jogo também é a base de um artigo acadêmico.
 
**Pergunta central:** *se a ação agressiva e poluidora é punida pela própria floresta, isso conscientiza mais o jogador?*
 
**Metodologia planejada:**
- **Telemetria** no Unity para contar ações poluidoras e ações ecológicas.
- **Teste A/B:** um grupo joga sem feedback de degradação e outro com todos os sistemas ativos.
- **Questionário pós-jogo** com escala Likert sobre a percepção de impacto ambiental.
## 🛠️ Tecnologias
 
- **Unity**
- **C#**
- Novo **Input System** do Unity (teclado e controle)

## 🚀 Como abrir o projeto
 
```bash
git clone https://github.com/pemito456-unity/Rotten-Rooots.git
```
 
1. Abra o **Unity Hub** e clique em **Add → Add project from disk**.
2. Selecione a pasta do repositório.
3. Abra com a versão do Unity indicada acima.
4. Abra a cena principal [PREENCHER: caminho da cena] e clique em **Play**.
## 📂 Estrutura do repositório
 
| Pasta | Conteúdo |
|---|---|
| `Assets/` | Scripts C#, cenas, sprites, áudio e demais recursos |
| `Packages/` | Pacotes do Unity |
| `ProjectSettings/` | Configurações do projeto Unity |
 
## 🧭 Roadmap
 
**Escopo da primeira entrega (J1) e build de playtest** *(marque o que já está pronto)*:
 
- [ ] Sistema de chunks (7 salas) com ativação e desativação de objetos
- [ ] Inventário de 8 slots com seleção por teclado e mouse
- [ ] Pistola como item no chão e contador de munição
- [ ] Ira da floresta (barra visível, aumento por disparos, mais inimigos e bloqueios)
- [ ] Kits de descontaminação
- [ ] Câmera e coleta de evidências (rolo limitado)
- [ ] Inimigos: morcego, onça e macaco neutro
- [ ] Lanterna com bateria
- [ ] Vida em coração, dano do jogador e condição de derrota
- [ ] Novo Input System (teclado e controle)
- [ ] Feedback sonoro e visual (tremor de tela e rugido na primeira eliminação)
**Planejado para as próximas entregas:**
 
- [ ] Efeito visual de borda vermelha e batida de coração com vida baixa
- [ ] Shaders (URP): área limpa com cores orgânicas, área estressada com névoa e distorção
- [ ] Telemetria e coleta de dados para o artigo
- [ ] Mais tipos de evidências e mais áreas do mapa

## 👤 Minha contribuição
 
[PREENCHER: o que você programou ou desenhou neste projeto, em 2 a 4 tópicos. Exemplo de formato:
- Programei [sistema] em C#...
- Implementei [mecânica]...]

## 👥 Equipe
 
[PREENCHER: nome e função de cada integrante]
 
## 🎨 Créditos e referências
 
**Assets**
- *Pixel HUD UI Free Kit*, por IndigoLay
- *Free Inventory System for 2D Games*, por Kinnly
- Efeitos sonoros: [Pixabay](https://pixabay.com)

**Referências de design**
- *Darkwood* e *Resident Evil 7*: atmosfera e tensão
- *The Last of Us*: body horror de fungo e mutação
- *Terra Nil*: lógica de limpar o mapa para evoluir
  
## 🎓 Contexto
 
Projeto desenvolvido no curso de **Design de Games** da **Universidade Anhembi Morumbi** (Projeto Final A3, 2026.2).
 
## 📫 Contato
 
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/pedro-thieghi-mamede)
[![Email](https://img.shields.io/badge/Email-EA4335?style=for-the-badge&logo=gmail&logoColor=white)](mailto:pedro.thieghi@outlook.com)
[![Itch.io](https://img.shields.io/badge/Itch.io-FA5C5C?style=for-the-badge&logo=itchdotio&logoColor=white)](https://pemito456.itch.io)
