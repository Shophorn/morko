







using UnityEngine;
using UnityEngine.UI;

public class DiscreteInputField : Selectable
{
	public enum Format { None, Int, Time }

	public Button decrementButton;
	public Button incrementButton;

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

	private void OnEnable()
	{
		incrementButton.onClick.AddListener(Increment);
		decrementButton.onClick.AddListener(Decrement);
	}

	private void OnDisable()
	{
		incrementButton.onClick.RemoveListener(Increment);
		decrementButton.onClick.RemoveListener(Decrement);
	}

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

		switch(format)
		{
			case Format.Int:
				valueDisplay.text = IntValue.ToString();
				break;

			case Format.Time:
				int minutes = IntValue / 60;
				int seconds = IntValue % 60;
				valueDisplay.text = $"{minutes.ToString("00")}:{seconds.ToString("00")}";
				break;

			default:
				valueDisplay.text = _value.ToString();
				break;
		}
	}

	private void Reset()
	{
		DetectComponents();
	}

	private void OnValidate()
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
}