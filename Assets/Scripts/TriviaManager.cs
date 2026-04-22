using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Collections;


public class TriviaManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(FetchQuestions("https://opentdb.com/api.php?amount=10"));
    }

    IEnumerator FetchQuestions(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var N = JSON.Parse(request.downloadHandler.text);
            Debug.Log(N.ToString());
            // Now you can access your data dynamically. For example:
            string value = N["results"][0]["type"].Value; // Now you can access JSON like in javascript with keys and 
            Debug.Log(value);

            for (int i = 0; i < 11; i++)
            {
                Debug.Log($"[Pregunta {i}] Categoría: {N["results"][i]["category"].Value}");
                Debug.Log($"[Pregunta {i}] Dificultad: {N["results"][i]["difficulty"].Value}");
                Debug.Log($"[Pregunta {i}] Pregunta: {N["results"][i]["question"].Value}");
                // Respuestas incorrectas
                string respuestas = "";
                for (int j = 0; j < N["results"][i]["incorrect_answers"].Count; j++)
                    respuestas += N["results"][i]["incorrect_answers"][j].Value + " | ";
                respuestas += N["results"][i]["correct_answer"].Value; // añade la correcta al final
                Debug.Log($"[Pregunta {i}] Todas las respuestas: {respuestas}");
                Debug.Log($"[Pregunta {i}] Respuesta correcta: {N["results"][i]["correct_answer"].Value}");
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
}

