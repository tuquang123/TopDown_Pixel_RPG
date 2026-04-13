
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class HoldButton : MonoBehaviour
{
    [SerializeField] private float holdDelay  = 0.4f;
    [SerializeField] private float repeatRate = 0.08f;

    public UnityEvent onHoldAction;

    private bool  _holding;
    private float _timer;
    private bool  _repeating;
    private RectTransform _rect;
    private Button _btn;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _btn  = GetComponent<Button>();
    }

    private void Update()
    {
        // Không làm gì nếu button bị disable
        if (!_btn.interactable) return;

        bool mouseDown = Input.GetMouseButton(0);
        bool overMe    = IsPointerOver();

        if (mouseDown && overMe)
        {
            if (!_holding)
            {
                _holding   = true;
                _timer     = 0f;
                _repeating = false;
                onHoldAction?.Invoke(); // Kích hoạt ngay lần đầu
            }
            else
            {
                _timer += Time.unscaledDeltaTime;

                if (!_repeating && _timer >= holdDelay)
                {
                    _repeating = true;
                    _timer = 0f;
                }

                if (_repeating && _timer >= repeatRate)
                {
                    _timer = 0f;
                    onHoldAction?.Invoke();
                }
            }
        }
        else
        {
            _holding   = false;
            _repeating = false;
        }
    }

    private bool IsPointerOver()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            _rect,
            Input.mousePosition,
            null
        );
    }
}