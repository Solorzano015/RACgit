using UnityEngine;
using System.Collections.Generic;

public class AnimatorTester : MonoBehaviour
{
    [Header("Referencia al Animator")]
    public Animator animator;

    public enum ParameterType
    {
        Boolean,
        Float,
        Switch,
        Trigger
    }

    [System.Serializable]
    public class KeyParameter
    {
        public KeyCode tecla1;
        public KeyCode tecla2 = KeyCode.None;

        public string parametro;
        public ParameterType tipo = ParameterType.Boolean;
        public float duracion = 1f;

        public bool bloquearAmbas = false;
        public bool invertir = false;

        [HideInInspector] public float valorActual = 0f;
        [HideInInspector] public bool estadoSwitch = false;
        [HideInInspector] public bool triggered = false;
        [HideInInspector] public float triggerTimer = 0f;

        public bool AreKeysPressed()
        {
            if (tecla2 == KeyCode.None) 
            {
                // Si no hay tecla 2, solo se considera la tecla 1
                return Input.GetKey(tecla1);
            }

            if (bloquearAmbas)
            {
                // En el modo bloquearAmbas, la tecla 2 no debe estar presionada
                return Input.GetKey(tecla1) && !Input.GetKey(tecla2);
            }

            // Nueva lógica:
            // 1. La tecla 1 debe estar presionada para que la tecla 2 sea considerada
            // 2. La tecla 2 no activa nada por sí sola
            if (Input.GetKey(tecla1))
            {
                if (tecla2 == KeyCode.None || Input.GetKey(tecla2))
                {
                    Debug.Log($"Parámetro activado: Tecla 1 ({tecla1}) + Tecla 2 ({tecla2})");
                    return true;
                }
            }
            
            return false;
        }



        public bool AreKeysReleased()
        {
            return Input.GetKeyUp(tecla1) || (tecla2 != KeyCode.None && Input.GetKeyUp(tecla2));
        }

        public bool AreKeysDown()
        {
            return Input.GetKeyDown(tecla1) || (tecla2 != KeyCode.None && Input.GetKeyDown(tecla2));
        }
    }

    [Header("Asignaciones de tecla y parámetro")]
    public List<KeyParameter> keyParameters = new List<KeyParameter>();

    void Update()
    {
        if (animator == null) return;

        foreach (KeyParameter kp in keyParameters)
        {
            bool keysPressed = kp.AreKeysPressed();
            bool keysReleased = kp.AreKeysReleased();
            bool keysDown = kp.AreKeysDown();

            switch (kp.tipo)
            {
                case ParameterType.Boolean:
                    animator.SetBool(kp.parametro, kp.invertir ? !keysPressed : keysPressed);
                    break;

                case ParameterType.Switch:
                    if (keysDown && !kp.triggered)
                    {
                        kp.estadoSwitch = !kp.estadoSwitch;
                        animator.SetBool(kp.parametro, kp.estadoSwitch);
                        kp.triggered = true;
                    }
                    if (keysReleased)
                    {
                        kp.triggered = false;
                    }
                    break;

                case ParameterType.Float:
                    if (keysPressed)
                    {
                        kp.valorActual += Time.deltaTime / kp.duracion;
                        kp.valorActual = Mathf.Clamp01(kp.valorActual);
                    }
                    else
                    {
                        kp.valorActual -= Time.deltaTime / kp.duracion;
                        kp.valorActual = Mathf.Clamp01(kp.valorActual);
                    }

                    animator.SetFloat(kp.parametro, kp.invertir ? 1f - kp.valorActual : kp.valorActual);
                    break;

                case ParameterType.Trigger:
                    if (keysDown && !kp.triggered)
                    {
                        animator.SetBool(kp.parametro, true);
                        kp.triggered = true;
                        kp.triggerTimer = kp.duracion;
                    }

                    if (kp.triggered)
                    {
                        kp.triggerTimer -= Time.deltaTime;
                        if (kp.triggerTimer <= 0f)
                        {
                            animator.SetBool(kp.parametro, false);
                            if (keysReleased) kp.triggered = false;
                        }
                    }
                    break;
            }
        }
    }
}
