﻿using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    [System.Serializable]
    private struct InputAxesConfiguration
    {
        public string MovementHorizontalAxisName;
        public string MovementVerticalAxisName;
        public string LookHorizontalAxisName;
        public string LookVerticalAxisName;
        public string AimDownSightsAxisName;
        public string AttackAxisName;
        public string JumpAxisName;
        public string ContextSensitiveActionAxisName;
    }

    private class InputData
    {
        public Vector2 Movement = Vector2.zero;
        public Vector2 Look = Vector2.zero;
        public bool AimDownSights = false;
        public bool Attack = false;
        public bool Jump = false;
        public bool ContextSensitiveAction = false;

        public bool IsEmpty
        {
            get
            {
                return Movement.x == 0.0f &&
                    Movement.y == 0.0f &&
                    Look.x == 0.0f &&
                    Look.y == 0.0f &&
                    AimDownSights == false &&
                    Attack == false &&
                    Jump == false &&
                    ContextSensitiveAction == false;
            }
        }
    }

    [SerializeField]
    private InputAxesConfiguration controllerConfig;
    [SerializeField]
    private InputAxesConfiguration keyboardMouseConfig;

    [Header("Component References")]
    [SerializeField]
    private Character player = null;

    private InputData emptyInput = new InputData();
    private InputData controllerInput = new InputData();
    private InputData keyboardMouseInput = new InputData();
    private InputData currentInput;

    public bool UsingControllerInput { get; private set; }

    public Vector2 Look
    {
        get
        {
            return currentInput.Look;
        }
    }

    public bool AimDownSights
    {
        get
        {
            return currentInput.AimDownSights;
        }
    }

    private void Awake()
    {
        UsingControllerInput = false;
    }

    private void Update()
    {
        currentInput = emptyInput;

        if (!Cursor.visible)
        {
            ReadInput(ref controllerInput, controllerConfig);
            ReadInput(ref keyboardMouseInput, keyboardMouseConfig);

            if (UsingControllerInput && !keyboardMouseInput.IsEmpty)
            {
                UsingControllerInput = false;
            }
            else if (!UsingControllerInput && !controllerInput.IsEmpty)
            {
                UsingControllerInput = true;
            }

            currentInput = (UsingControllerInput ? controllerInput : keyboardMouseInput);
        }

        // Move.
        Vector3 steeringDirection = new Vector3(currentInput.Movement.x, 0.0f, currentInput.Movement.y);
        steeringDirection = transform.TransformDirection(steeringDirection);
        player.Steering.SetTarget(transform.position + steeringDirection, 0.0f);

        // Jump.
        if (currentInput.Jump)
        {
            player.Steering.Jump();
        }

        if (currentInput.Attack)
        {
            player.MeleeAttack.Attack();
        }

        // Context sensitive action.
        if (currentInput.ContextSensitiveAction && player.Health.IsDead)
        {
            Transform spawnPoint = GameplayManager.Instance.CurrentLevel.GetClosestSpawnPoint(transform.position);
            player.Health.Spawn(spawnPoint);
        }
    }

    private void ReadInput(ref InputData data, InputAxesConfiguration config)
    {
        data.Movement = new Vector2(
            Input.GetAxis(config.MovementHorizontalAxisName),
            Input.GetAxis(config.MovementVerticalAxisName));

        data.Look = new Vector2(
            Input.GetAxis(config.LookHorizontalAxisName),
            Input.GetAxis(config.LookVerticalAxisName));

        data.AimDownSights = Input.GetAxis(config.AimDownSightsAxisName) > 0.0f;

        data.Attack = Input.GetAxis(config.AttackAxisName) > 0.0f;

        data.Jump = Input.GetAxis(config.JumpAxisName) > 0.0f;
    }
}
