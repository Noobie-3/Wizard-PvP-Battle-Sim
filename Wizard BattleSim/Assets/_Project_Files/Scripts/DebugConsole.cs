using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ConsoleCommandAttribute : Attribute
{
    public string Description;
    public ConsoleCommandAttribute(string description = "")
    {
        Description = description;
    }
}

public class DebugConsole : MonoBehaviour
{
    public GameObject consoleUI;
    public InputField inputField;
    public Text outputText;
    public Text suggestionText;

    private Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
    private Dictionary<string, object> commandInstances = new Dictionary<string, object>();
    private List<string> commandList = new List<string>();
    private List<string> currentSuggestions = new List<string>();
    private int suggestionIndex = 0;
    private string currentInput = "";

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        consoleUI.SetActive(false);
        RegisterCommands();
        inputField.onValueChanged.AddListener(UpdateAutoComplete);
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) // Toggle console with ` key
        {
            consoleUI.SetActive(!consoleUI.activeSelf);
            if(consoleUI.activeSelf)
            {
                // Select the input field when the console is opened
                EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                inputField.OnPointerClick(new PointerEventData(EventSystem.current));
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab)) // Auto-fill with the current suggestion
        {
            AutoFillCommand();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) // Cycle through suggestions
        {
            CycleAutoComplete(Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 1);
        }
    }

    void RegisterCommands()
    {
        commands.Clear();
        commandInstances.Clear();
        commandList.Clear();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (method.GetCustomAttribute<ConsoleCommandAttribute>() != null)
                {
                    string commandName = method.Name.ToLower();
                    commands[commandName] = method;
                    commandList.Add(commandName);

                    if (!method.IsStatic)
                    {
                        object instance = FindObjectOfType(type) ?? Activator.CreateInstance(type);
                        commandInstances[commandName] = instance;
                    }
                }
            }
        }
    }

    public void ExecuteCommand()
    {
        string[] inputParts = inputField.text.ToLower().Split(' ');
        string commandName = inputParts[0];
        string[] args = inputParts.Length > 1 ? inputParts[1..] : new string[0];

        if (commands.TryGetValue(commandName, out MethodInfo method))
        {
            object instance = commandInstances.ContainsKey(commandName) ? commandInstances[commandName] : null;
            ParameterInfo[] parameters = method.GetParameters();

            object[] parsedArgs = new object[parameters.Length];
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                }
                method.Invoke(instance, parsedArgs);
                outputText.text += $"\n> {commandName} executed.";
            }
            catch (Exception e)
            {
                outputText.text += $"\nError executing command: {e.Message}";
            }
        }
        else
        {
            outputText.text += $"\nUnknown command: {commandName}";
        }

        inputField.text = "";
        suggestionText.text = "";
        currentSuggestions.Clear();
        suggestionIndex = 0;
    }

    void UpdateAutoComplete(string input)
    {
        currentInput = input;
        if (string.IsNullOrWhiteSpace(input))
        {
            suggestionText.text = "";
            currentSuggestions.Clear();
            suggestionIndex = 0;
            return;
        }

        currentSuggestions = commandList.Where(cmd => cmd.StartsWith(input.ToLower())).ToList();
        suggestionIndex = 0;
        suggestionText.text = currentSuggestions.Count > 0 ? $"Suggested: {currentSuggestions[0]}" : "";
    }

    void AutoFillCommand()
    {
        if (currentSuggestions.Count > 0)
        {
            inputField.text = currentSuggestions[suggestionIndex];
            inputField.caretPosition = inputField.text.Length;
            suggestionText.text = "";
        }
    }

    void CycleAutoComplete(int direction)
    {
        if (currentSuggestions.Count == 0) return;

        suggestionIndex = (suggestionIndex + direction + currentSuggestions.Count) % currentSuggestions.Count;
        suggestionText.text = $"Suggested: {currentSuggestions[suggestionIndex]}";
    }
}
