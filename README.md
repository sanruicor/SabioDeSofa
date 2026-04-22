# 🎯 TrivialAPI

Juego de trivia desarrollado en **Unity 6** para Android que consume la API pública [Open Trivia Database](https://opentdb.com/).

## 🕹️ ¿En qué consiste?

TrivialAPI es un juego de preguntas y respuestas en el que el jugador elige una categoría y responde 10 preguntas obtenidas en tiempo real desde la API. Cada pregunta tiene una dificultad asociada que determina los puntos que se obtienen al acertar.

| Dificultad | Puntos |
|------------|--------|
| Fácil      | 1      |
| Media      | 2      |
| Difícil    | 3      |

Al finalizar las 10 preguntas se muestra la puntuación total obtenida sobre el máximo posible.

## 📱 Flujo de juego

1. El jugador selecciona una **categoría** (Ciencia, Historia, Videojuegos, etc.)
2. Se cargan **10 preguntas** desde la API
3. Cada pregunta muestra sus respuestas en orden aleatorio
4. Tras seleccionar una respuesta se indica si es **correcta o incorrecta**
5. Al terminar se muestra la **puntuación final**
