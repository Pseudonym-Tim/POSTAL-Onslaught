using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class LevelScriptParser
{
    private static readonly string FILE_NAME = "level_generation";
    private static readonly string FILE_EXTENSION = ".level";

    private readonly LevelManager levelManager;
    private readonly LevelGenerator levelGenerator;
    private readonly Dictionary<string, int> variables = new Dictionary<string, int>();
    private string[] scriptLines;
    private int currentLineIndex;

    public int LevelSizeX { get; private set; }
    public int LevelSizeY { get; private set; }
    public int LevelBoundsDist { get; private set; }

    public LevelScriptParser(LevelManager levelManager, LevelGenerator levelGenerator)
    {
        this.levelManager = levelManager;
        this.levelGenerator = levelGenerator;
    }

    public void ParseScript()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, FILE_NAME + FILE_EXTENSION);

        try
        {
            scriptLines = File.ReadAllLines(filePath);
            currentLineIndex = 0;

            while(currentLineIndex < scriptLines.Length)
            {
                ParseLine(scriptLines[currentLineIndex].Trim());
                currentLineIndex++;
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"Error parsing script file: {e.Message}");
        }
    }

    public void ParseLine(string line)
    {
        if(string.IsNullOrEmpty(line) || line.StartsWith("//")) return;

        if(line.StartsWith("VAR")) ParseVariable(line);
        else if(line.StartsWith("FOR")) ParseForLoop(line);
        else if(line.StartsWith("IF")) ParseIfStatement(line);
        else if(line.StartsWith("TILE ")) ParseTile(line);
        else if(line.StartsWith("TILE_FILL")) ParseTileFill(line);
        else if(line.StartsWith("ENTITY ")) ParseEntity(line);
        else if(line.StartsWith("ENTITY_RANDOM ")) ParseEntityRandom(line);
    }

    private void ParseVariable(string line)
    {
        var parts = line.Split(' ');
        string varName = parts[1];
        int value = int.Parse(parts[2]);

        variables[varName] = value;

        if(varName == "LEVEL_SIZE_X") LevelSizeX = value;
        if(varName == "LEVEL_SIZE_Y") LevelSizeY = value;
        if(varName == "LEVEL_BOUNDS_DIST") LevelBoundsDist = value;
    }

    private void ParseForLoop(string line)
    {
        var parts = line.Split(' ');
        string varName = parts[1];
        int start = GetValue(parts[2]);
        int end = GetValue(parts[3]);

        int loopStartIndex = currentLineIndex;
        int loopEndIndex = FindMatchingEnd(loopStartIndex, "FOR", "ENDFOR");

        for(int i = start; i <= end; i++)
        {
            variables[varName] = i;
            ExecuteBlock(loopStartIndex + 1, loopEndIndex - 1);
        }

        currentLineIndex = loopEndIndex;
    }

    private int FindMatchingEnd(int startIndex, string startToken, string endToken)
    {
        int nestedLevels = 1;

        for(int i = startIndex + 1; i < scriptLines.Length; i++)
        {
            string trimmedLine = scriptLines[i].Trim();
            if(trimmedLine.StartsWith(startToken)) nestedLevels++;
            else if(trimmedLine.StartsWith(endToken)) nestedLevels--;
            if(nestedLevels == 0) return i;
        }

        throw new Exception($"{endToken} not found.");
    }

    private void ParseIfStatement(string line)
    {
        var condition = line.Substring(3).Trim();
        bool conditionMet = EvaluateCondition(condition);

        int ifEndIndex = FindMatchingEnd(currentLineIndex, "IF", "ENDIF");
        int elseIndex = FindElseIndex(currentLineIndex, ifEndIndex);

        if(conditionMet)
        {
            ExecuteBlock(currentLineIndex + 1, elseIndex == -1 ? ifEndIndex - 1 : elseIndex - 1);
        }
        else if(elseIndex != -1)
        {
            ExecuteBlock(elseIndex + 1, ifEndIndex - 1);
        }

        currentLineIndex = ifEndIndex;
    }

    private int FindElseIndex(int startIndex, int endIndex)
    {
        for(int i = startIndex + 1; i < endIndex; i++)
        {
            if(scriptLines[i].Trim().StartsWith("ELSE")) return i;
        }

        return -1;
    }

    private void ExecuteBlock(int startIndex, int endIndex)
    {
        for(int i = startIndex; i <= endIndex; i++)
        {
            ParseLine(scriptLines[i].Trim());
        }
    }

    private void ParseTile(string line)
    {
        var parts = line.Split(' ');
        string tileName = parts[1];
        int x = GetValue(parts[2]);
        int y = GetValue(parts[3]);
        levelManager.AddTile(tileName, new Vector2(x, y));
    }

    private void ParseTileFill(string line)
    {
        var parts = line.Split(' ');
        string tileName = parts[1];
        int x1 = GetValue(parts[2]);
        int y1 = GetValue(parts[3]);
        int x2 = GetValue(parts[4]);
        int y2 = GetValue(parts[5]);

        for(int x = x1; x <= x2; x++)
        {
            for(int y = y1; y <= y2; y++)
            {
                levelManager.AddTile(tileName, new Vector2(x, y));
            }
        }
    }

    private void ParseEntity(string line)
    {
        var parts = line.Split(' ');
        string entityID = parts[1];
        int x = GetValue(parts[2]);
        int y = GetValue(parts[3]);
        levelGenerator.SpawnEntity(entityID, new Vector2(x, y));
    }

    private void ParseEntityRandom(string line)
    {
        var parts = line.Split(' ');
        string entityID = parts[1];
        string spawnTileID = parts[2];
        int minBoundsDist = GetValue(parts[3]);
        levelGenerator.SpawnEntity(entityID, spawnTileID, minBoundsDist);
    }

    private int GetValue(string token)
    {
        token = token.Trim();

        if(variables.ContainsKey(token)) return variables[token];
        if(int.TryParse(token, out int value)) return value;

        var match = Regex.Match(token, @"(\d+|\w+)\s*([\+\-])\s*(\d+|\w+)");

        if(match.Success)
        {
            int left = GetValue(match.Groups[1].Value);
            int right = GetValue(match.Groups[3].Value);
            return match.Groups[2].Value == "+" ? left + right : left - right;
        }

        throw new FormatException($"Invalid token format: {token}");
    }

    private bool EvaluateCondition(string condition)
    {
        var orParts = condition.Split(new[] { " OR " }, StringSplitOptions.None);

        foreach(var orPart in orParts)
        {
            if(EvaluateSubCondition(orPart.Trim())) return true;
        }

        return false;
    }

    private bool EvaluateSubCondition(string condition)
    {
        var andParts = condition.Split(new[] { " AND " }, StringSplitOptions.None);

        foreach(var andPart in andParts)
        {
            if(!EvaluateExpression(andPart.Trim())) return false;
        }

        return true;
    }

    private bool EvaluateExpression(string expression)
    {
        string[] operators = { "==", "!=", "<=", ">=", "<", ">" };

        foreach(var op in operators)
        {
            var parts = expression.Split(new[] { op }, StringSplitOptions.None);

            if(parts.Length == 2)
            {
                int left = GetValue(parts[0].Trim());
                int right = GetValue(parts[1].Trim());
                return op switch
                {
                    "==" => left == right,
                    "!=" => left != right,
                    "<=" => left <= right,
                    ">=" => left >= right,
                    "<" => left < right,
                    ">" => left > right,
                    _ => throw new FormatException($"Invalid operator: {op}")
                };
            }
        }

        throw new FormatException($"Invalid expression format: {expression}");
    }
}