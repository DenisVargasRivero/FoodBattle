﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {

    //Esta es la forma correcta de mostrar variables privadas en el inspector. 
    //No se deben hacer public variables que no queremos sean accesibles desde otras clases-
    [SerializeField]
    string sceneToLoad;
    [SerializeField]
    Text percentText;
    [SerializeField]
    Image progressImage;

	// En cuanto se active el objeto, se inciará el cambio de escena
	void Start () {
        //Iniciamos una corrutina, que es un método que se ejecuta 
        //en una línea de tiempo diferente al flujo principal del programa
        StartCoroutine(LoadScene());
	}

	//Corrutina
	IEnumerator LoadScene()
	{
		AsyncOperation loading;

		//Iniciamos la carga asíncrona de la escena y guardamos el proceso en 'loading'
		loading = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
		//Bloqueamos el salto automático entre escenas para asegurarnos el control durante el proceso
		//loading.allowSceneActivation = false;


		//Cuando la escena llega al 90% de carga, se produce el cambio de escena
		while (!loading.isDone)
        {
			
			//Actualizamos el % de carga de una forma optima 
			//(concatenar con + tiene un alto coste en el rendimiento)
            float progress = Mathf.Clamp01(loading.progress / .9f);
			percentText.text = progress * 100f + "%";
            Debug.Log(progress);
			//Actualizamos la barra de carga
			progressImage.fillAmount = progress;

			//Esperamos un frame
			yield return new WaitForEndOfFrame();
		}

        //Mostramos la carga como finalizada

        /*percentText.text = "100%";

        progressImage.fillAmount = 1;*/
       
        //Activamos el salto de escena.

        loading.allowSceneActivation = true;
    }
}


