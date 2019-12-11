using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class DiscreteInputField : Selectable
{
	public enum Format { None, Int, Time }

	[SerializeField] private Button decrementButton;
	[SerializeField] private Button incrementButton;

	[System.Serializable] public class FloatEvent : UnityEvent<float> {}
	public FloatEvent OnValueChanged;

	[SerializeField] private float _value;
	public float Value
	{
		get => _value;
		set
		{
			_value = value;
			RefreshValue();
		}
	}

	public int IntValue => (int)_value;

	public Text valueDisplay;
	public float step = 1.0f;
	public float minValue = 0.0f;
	public float maxValue = 1.0f;

	public Format format;

	protected override void Awake()
	{
		incrementButton.onClick.AddListener(Increment);
		decrementButton.onClick.AddListener(Decrement);
	}

	private void Decrement()
	{
		_value -= step;
		RefreshValue();
	}

	private void Increment()
	{ 
		_value += step;
		RefreshValue();
	}

	/* Todo(Leo, Joonas): Maybe we could use this to use arrows for decrement and increment
	Also use this.Select and OnSelect (on increment and decrement buttons) to select this
	when using mouse to hover. See https://docs.unity3d.com/2017.4/Documentation/ScriptReference/UI.Selectable.html
	for more details */
	// public override Selectable FindSelectableOnLeft()
	// {
	// 	Decrement();
	// 	return this as Selectable;		
	// }


	/* Note(Leo): This function validates value and sets current
	value on display obeying format rules. */
	private void RefreshValue()
	{	
		bool isIntegerFormat = (format == Format.Int) || (format == Format.Time);
		if (isIntegerFormat)
		{
			_value = (int)_value;
			minValue = (int)minValue;
			maxValue = (int)maxValue;
		}

		_value = Mathf.Clamp(_value, minValue, maxValue);

		decrementButton.enabled = _value > minValue;
		incrementButton.enabled = _value < maxValue;

		switch(format)
		{
			case Format.Int:
				valueDisplay.text = IntValue.ToString();
				break;

			case Format.Time:
				valueDisplay.text = TimeFormat.ToTimeFormat(IntValue);
				break;

			default:
				valueDisplay.text = _value.ToString();
				break;
		}

		OnValueChanged?.Invoke(_value);
	}

	#if UNITY_EDITOR
	protected override void Reset()
	{
		DetectComponents();
	}

	protected override void OnValidate()
	{
		RefreshValue();
	}

	private void DetectComponents()
	{
		for(int childIndex = 0; childIndex < transform.childCount; childIndex++)
		{
			var child = transform.GetChild(childIndex);
			switch(child.name)
			{
				case "Decrement":
					decrementButton = child.GetComponent<Button>();
					break;

				case "Increment":
					incrementButton = child.GetComponent<Button>();
					break;

				case "ValueDisplay":
					valueDisplay = child.GetComponent<Text>();
					break;

			}
		}
	}
	#endif
}