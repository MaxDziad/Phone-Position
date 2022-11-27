using Runtime.Utils;
using UnityEngine;

public class CurrentActionSoundInvoker : MonoBehaviour
{
    [SerializeField]
    private CurrentActionDataProvider _actionDataProvider;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip _unknownSound;

    [SerializeField]
    private AudioClip _walkingSound;

    [SerializeField]
    private AudioClip _walkingUpstairsSound;

    [SerializeField]
    private AudioClip _walkingDownstairsSound;

    [SerializeField]
    private AudioClip _sittingSound;

    [SerializeField]
    private AudioClip _standingSound;

    [SerializeField]
    private AudioClip _layingSound;

    private void Awake()
    {
        if (_actionDataProvider != null)
        {
            _actionDataProvider.OnCurrentActionTypeChangedEvent += OnCurrentActionTypeChanged;
        }
    }

    private void OnDestroy()
    {
        if (_actionDataProvider != null)
        {
            _actionDataProvider.OnCurrentActionTypeChangedEvent -= OnCurrentActionTypeChanged;
        }
    }

    private void OnCurrentActionTypeChanged(ModelActionType newActionType)
    {
        TryPlayNewActionTypeSound(newActionType);
    }

    private void TryPlayNewActionTypeSound(ModelActionType newActionType)
    {
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(GetActionTypeClip(newActionType));
        }
    }

    private AudioClip GetActionTypeClip(ModelActionType actionType)
    {
        return actionType switch
        {
            ModelActionType.Unknown => _unknownSound,
            ModelActionType.Laying => _layingSound,
            ModelActionType.Standing => _standingSound,
            ModelActionType.Walking => _walkingSound,
            ModelActionType.WalkingUpstairs => _walkingUpstairsSound,
            ModelActionType.WalkingDownstairs => _walkingDownstairsSound,
            ModelActionType.Sitting => _sittingSound,
            _ => null
        };
    }
}
