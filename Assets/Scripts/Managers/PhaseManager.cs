using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class PhaseManager : MonoBehaviourPunCallbacks
{
    public static PhaseManager Instance { get; private set; }

    [Header("Phase Settings")]
    [SerializeField] private float dayDuration = 300f;
    [SerializeField] private float nightDuration = 180f;
    [SerializeField] private float votingDuration = 60f;
    [SerializeField] private float transitionDuration = 3f;

    [Header("Phase Effects")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Color dayLightColor = Color.white;
    [SerializeField] private Color nightLightColor = new Color(0.1f, 0.1f, 0.2f);
    [SerializeField] private float dayLightIntensity = 1f;
    [SerializeField] private float nightLightIntensity = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip dayAmbience;
    [SerializeField] private AudioClip nightAmbience;
    [SerializeField] private AudioClip votingMusic;
    [SerializeField] private AudioClip phaseTransitionSound;

    private GameManager.GamePhase currentPhase = GameManager.GamePhase.Waiting;
    private float phaseTimer = 0f;
    private float transitionTimer = 0f;
    private bool isTransitioning = false;
    private AudioSource audioSource;
    private List<BaseRoleBehaviour> roleBehaviours = new List<BaseRoleBehaviour>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartDayPhase();
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (isTransitioning)
        {
            HandlePhaseTransition();
        }
        else
        {
            UpdatePhaseTimer();
        }
    }

    private void UpdatePhaseTimer()
    {
        phaseTimer -= Time.deltaTime;
        if (phaseTimer <= 0f)
        {
            StartPhaseTransition();
        }
    }

    private void StartPhaseTransition()
    {
        isTransitioning = true;
        transitionTimer = transitionDuration;
        PlayPhaseTransitionSound();
    }

    private void HandlePhaseTransition()
    {
        transitionTimer -= Time.deltaTime;
        if (transitionTimer <= 0f)
        {
            isTransitioning = false;
            AdvancePhase();
        }
    }

    private void AdvancePhase()
    {
        switch (currentPhase)
        {
            case GameManager.GamePhase.Day:
                StartNightPhase();
                break;
            case GameManager.GamePhase.Night:
                StartVotingPhase();
                break;
            case GameManager.GamePhase.Voting:
                StartDayPhase();
                break;
        }
    }

    public void StartDayPhase()
    {
        currentPhase = GameManager.GamePhase.Day;
        phaseTimer = dayDuration;
        UpdateLighting(dayLightColor, dayLightIntensity);
        PlayPhaseAudio(dayAmbience);
        NotifyRoleBehaviours();
    }

    public void StartNightPhase()
    {
        currentPhase = GameManager.GamePhase.Night;
        phaseTimer = nightDuration;
        UpdateLighting(nightLightColor, nightLightIntensity);
        PlayPhaseAudio(nightAmbience);
        NotifyRoleBehaviours();
    }

    public void StartVotingPhase()
    {
        currentPhase = GameManager.GamePhase.Voting;
        phaseTimer = votingDuration;
        PlayPhaseAudio(votingMusic);
        NotifyRoleBehaviours();
    }

    private void UpdateLighting(Color color, float intensity)
    {
        if (directionalLight != null)
        {
            directionalLight.color = color;
            directionalLight.intensity = intensity;
        }
    }

    private void PlayPhaseAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void PlayPhaseTransitionSound()
    {
        if (audioSource != null && phaseTransitionSound != null)
        {
            audioSource.PlayOneShot(phaseTransitionSound);
        }
    }

    private void NotifyRoleBehaviours()
    {
        foreach (var behaviour in roleBehaviours)
        {
            switch (currentPhase)
            {
                case GameManager.GamePhase.Day:
                    behaviour.OnDayPhaseStart();
                    break;
                case GameManager.GamePhase.Night:
                    behaviour.OnNightPhaseStart();
                    break;
                case GameManager.GamePhase.Voting:
                    behaviour.OnVotingPhaseStart();
                    break;
            }
        }
    }

    public void RegisterRoleBehaviour(BaseRoleBehaviour behaviour)
    {
        if (!roleBehaviours.Contains(behaviour))
        {
            roleBehaviours.Add(behaviour);
        }
    }

    public void UnregisterRoleBehaviour(BaseRoleBehaviour behaviour)
    {
        roleBehaviours.Remove(behaviour);
    }

    public GameManager.GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public float GetPhaseTimeRemaining()
    {
        return phaseTimer;
    }

    public float GetTransitionTimeRemaining()
    {
        return transitionTimer;
    }
} 