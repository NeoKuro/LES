using System.Collections.Generic;
using B83.ExpressionParser;
//using UnityEngine;

public struct Node
{
    private char value;                 //Contents of the node (*+-/#AB)
    private int treeSize;               //Total size of the nodeTree. Only the root node uses this
    private int nMax;                   //NMax = number of arguments needed (0, 1, or 2)
    private int charIndex;              //Store char index - used to look ahead to next corresponding value
    private int relativeIndex;          //Char Index but relative to the 'used' gene string (shortened)
    private List<Node> parentNode;      //Stores a reference to the parent node. 
                                        //If node is the 'rootNode' (first node in sequence) then parentNode will be a 'nullptr'
    private List<Node> childNodes;      //Any child node attached to this node (can be 0, 1 or 2)

    //BlankNode Constructor
    public Node(char val)
    {
        parentNode = new List<Node>();
        value = val;
        treeSize = 1;
        nMax = 999;
        charIndex = -1;
        relativeIndex = 999;
        childNodes = new List<Node>();
    }

    //RootNode Constructor
    public Node(char thisValue, string gene, int relIndex)
    {
        NodeManager.phenoCount++;
        parentNode = new List<Node>();
        value = thisValue;
        nMax = 0;
        charIndex = NodeManager.phenoCount;
        treeSize = charIndex;
        relativeIndex = relIndex;
        childNodes = new List<Node>();
        nMax = CalculateNMax();
    }

    //ChildNode Constructor
    public Node(char thisValue, string gene, int relIndex, Node parent)
    {
        NodeManager.phenoCount++;
        parentNode = new List<Node>();
        parentNode.Add(parent);
        value = thisValue;
        nMax = 0;
        charIndex = NodeManager.phenoCount;
        treeSize = charIndex;
        relativeIndex = relIndex;
        childNodes = new List<Node>();
        nMax = CalculateNMax();
    }

    //Build the tree Top to bottom, Left to right.
    //Will build the left nodes/tree first before progressing to the right nodes
    public void CompletePhenotype(string gene)
    {
        //If it is A or B no new nodes are attached
        if (nMax == 0)
        {
            return;
        }

        if (nMax == 1)
        {
            string thisGene = gene;
            int opCount = 1;
            for (int i = 0; i <= relativeIndex; i++)
            {
                if (thisGene[i] == '+' || thisGene[i] == '-' || thisGene[i] == '*' || thisGene[i] == '/')
                {
                    opCount++;
                }
            }
            int nextIndex = relativeIndex + opCount;

            Node newNode = new Node(thisGene[nextIndex], thisGene, nextIndex, this);
            newNode.CompletePhenotype(thisGene);
            childNodes.Add(newNode);
            return;
        }

        //Otherwise if it is */+- (most likely) then there will be 2 nodes attached
        if (nMax == 2)
        {
            //To get the left skip ahead however many nMax=2 operators there are before this one (including this one)
            //If there was an nMax=1 (#), do not count the # in the operators count.
            //If there was an nMax=0 (A,B), cut out the previous code and 'start' fresh as it were.


            string thisGene = gene;
            int opCount = 0;
            for (int i = 0; i <= relativeIndex; i++)
            {
                if (thisGene[i] == '+' || thisGene[i] == '-' || thisGene[i] == '*' || thisGene[i] == '/')
                {
                    opCount++;
                }
                if (thisGene[i] == 'A' || thisGene[i] == 'B')
                {
                    //If A or B then manually remove each char until the correct amount have been removed
                    for (int j = 0; j < relativeIndex; j++)
                    {
                        string modifiedGeneStr = thisGene.Remove(j, 1);
                        thisGene = modifiedGeneStr;
                    }
                    //This Node is now the start of a 'new' branch 
                    //So set relativeIndex to 0 (new)
                    //      And make sure set opCount to 1 for below
                    relativeIndex = 0;
                    opCount = 1;
                }
            }


            int leftNodeIndex = relativeIndex + opCount;



            //Add the next Left Node
            Node newLeftNode = new Node(thisGene[leftNodeIndex], thisGene, leftNodeIndex, this);
            newLeftNode.CompletePhenotype(thisGene);
            childNodes.Add(newLeftNode);

            int rightNodeIndex = leftNodeIndex + 1;

            Node newRightNode = new Node(thisGene[rightNodeIndex], thisGene, rightNodeIndex, this);
            newRightNode.CompletePhenotype(thisGene);
            childNodes.Add(newRightNode);
            return;
        }
    }

    public char GetValue()
    {
        return value;
    }

    public int GetTreeSize()
    {
        return treeSize;
    }

    public int GetNMax()
    {
        return nMax;
    }

    public int GetIndex()
    {
        return charIndex;
    }

    public List<Node> GetParentNode()
    {
        //Should either have nothing there or 1 item.
        if (parentNode.Count > 0)
            return parentNode;

        //return Nothing
        return null;
    }

    public List<Node> GetChildNodes()
    {
        if (childNodes.Count > 0)
            return childNodes;

        return null;
    }

    public int GetChildCount()
    {
        return childNodes.Count;
    }

    public Node GetLeftNode()
    {
        if (nMax < 1)
        {
            //Debug.Log("There are no nodes attached to this Node");
            return new Node('|');
        }

        return childNodes[0];   //The first/only node will be considered the left node
    }

    public Node GetRightNode()
    {
        if (nMax < 2)
        {
            //Debug.Log("There is no second node attached to this Node");
            return new Node('|');
        }

        return childNodes[1];
    }

    private int CalculateNMax()
    {
        switch (value)
        {
            case '*':
                return nMax = 2;
            case '/':
                return nMax = 2;
            case '-':
                return nMax = 2;
            case '+':
                return nMax = 2;
            case '#':
                return nMax = 1;
            case 'A':
                return nMax = 0;
            case 'B':
                return nMax = 0;
        }
        return 0;
    }
}

public struct EquationData
{
    public char symbol;
    public int index;
    public int jumpToIndex;

    public EquationData(char sym, int id)
    {
        symbol = sym;
        index = id;
        jumpToIndex = -1;
    }
}

public class NodeManager //: MonoBehaviour
{
    //Each item in a genome CAN be a 'node'
    //However only those of the phenotype will be created/displayed
    //Can later attach a 'gameobject' to each 'type' of node (A, B, *....) and render in engine

    //Custom-Struct Public
    public Dictionary<int, List<Node>> chromosomeRootNodes;      //Root Nodes of each chromosome (In theory, the other nodes should be connected through the children)
                                                                 //Root nodes are the first nodes of each gene in a chromosome (IE each 
                                                                 //Standard public
    public Dictionary<int, List<string>> chromosomePhenotypeGeneEquations;                                  //Stores the final built equations of each gene in each chromosome
    public Dictionary<int, List<float>> chromosomePhenotypeGeneResults;                                         //Stores the values of the Per Gene Evaluations

    //Standard Static Public
    public static int phenoCount = -1;          //Reprensents the index of a node in a phenotype, also reprensents the total length of the phenotype of that gene

    //Standard Public
    public List<string> chromosomePhenotypeChromoEquations;                                      //Stores the final built equations of each chromosome (genes linked)
    public List<float> chromosomePhenotypeChromoResults;                                         //Stores the values of the Linked Gene Evaluations

    //Custom-Struct Private
    private Dictionary<int, List<Node>> equationGroups = new Dictionary<int, List<Node>>();              //Used to check whether a node is unique, or can be referenced elsewhere in the tree
    private Dictionary<int, List<EquationData>> equation = new Dictionary<int, List<EquationData>>();    //Stores the equation equation data and links the duplicate indices
    ExpressionParser parser = new ExpressionParser();


    //Create Expression Trees for each GENE
    public void GeneratePhenotype(Chromosome[] genome)
    {
        chromosomeRootNodes = new Dictionary<int, List<Node>>();
        //For each chromosome
        for (int i = 0; i < genome.Length - 1; i++)
        {
            chromosomeRootNodes.Add(i, new List<Node>());
            //For each gene in chromosome
            for (int j = 0; j < genome[i].genes.Length; j++)
            {
                phenoCount = -1;
                string thisGene = genome[i].genes[j].gene;
                Node newNode = new Node(thisGene[0], thisGene, phenoCount + 1);
                newNode.CompletePhenotype(thisGene);
                chromosomeRootNodes[i].Add(newNode);
            }
        }
    }

    public void GeneratePhenotypeEquation()
    {
        chromosomePhenotypeGeneEquations = new Dictionary<int, List<string>>();
        
        //Foreach chromosome
        foreach (KeyValuePair<int, List<Node>> nodeTree in chromosomeRootNodes)
        {
            chromosomePhenotypeGeneEquations.Add(nodeTree.Key, new List<string>());

            //For each nodeTree in that chromosome, create the equation groups
            for (int i = 0; i < nodeTree.Value.Count; i++)
            {
                Node rootNode = nodeTree.Value[i];
                Node thisNode = rootNode;

                //Sorts equations into groups of 1 operator and 1-2 elements. Also links the groups in the dictionary when they are referencing each other
                //  (IE Index 0 has + / A, a link to index 1 will be made for the '/' )
                GetEquationGroups(thisNode);
                string equationStr = StartEquation(0);
                chromosomePhenotypeGeneEquations[nodeTree.Key].Add(equationStr);

                equationGroups.Clear();         //EquationGroups is used for each nodeTree so must be cleared each time. The result should be stored in ChromosomePhenotypeEquations
                equation.Clear();
            }
        }
    }

    public void EvaluatePhenotypeEquations(CreatureManager hManager)
    {
        parser.AddConst("A", () => GlobalGEPSettings.A_VAL);
        parser.AddConst("B", () => GlobalGEPSettings.B_VAL);
        chromosomePhenotypeChromoEquations = new List<string>();
        chromosomePhenotypeGeneResults = new Dictionary<int, List<float>>();
        chromosomePhenotypeChromoResults = new List<float>();
        foreach (KeyValuePair<int, List<string>> chromosomeEquation in chromosomePhenotypeGeneEquations)
        {
            chromosomePhenotypeGeneResults.Add(chromosomeEquation.Key, new List<float>());
            for (int i = 0; i < chromosomeEquation.Value.Count; i++)
            {
                //Evaluates the genes individually rather than linking them
                //If traits are on genes rather than chromosomes then no need to link all the genes and evaluate entire chromosomes together
                if (GlobalGEPSettings.TRAITS_ON_GENES)
                {
                    Expression expressionResult = parser.EvaluateExpression(chromosomeEquation.Value[i]);
                    float result = (float)expressionResult.Value;
                    //Check to make sure only usable numbers are present.
                    if (float.IsNaN(result) || float.IsInfinity(result) || float.IsNegativeInfinity(result))
                    {
                        result = 0;
                    }
                    chromosomePhenotypeGeneResults[chromosomeEquation.Key].Add(result);
                }
            }

            if (!GlobalGEPSettings.TRAITS_ON_GENES)
            {
                string linkedEquation = LinkEquations(chromosomeEquation.Value, hManager.generator);
                Expression expressionResult = parser.EvaluateExpression(linkedEquation);
                float result = (float)expressionResult.Value;
                //Check to make sure only usable numbers are present.
                if (float.IsNaN(result) || float.IsInfinity(result) || float.IsNegativeInfinity(result))
                {
                    result = 0;
                }
                chromosomePhenotypeChromoResults.Add(result);
            }
        }
    }

    //Sorts the Expression trees into usable equation groups where a root node (parent) and it's children (if any) are sorted into a group to later be 
    //  built into an equation for evaluation
    private void GetEquationGroups(Node rootNode)
    {
        Node thisNode = rootNode;
        List<Node> nodeData = new List<Node>();
        List<EquationData> equationData = new List<EquationData>();
        int keys = equation.Keys.Count;

        //Make sure that the node has children. If it is an element ignore it to prevent adding it twice
        //Alternatively, if it is the root/first node in the tree add it anyway
        if (thisNode.GetChildCount() > 0 || thisNode.GetIndex() == 0)
        {
            //Add a new entry into the dictionary and start populating the 'nodeData' list with this current node
            equationGroups.Add(keys, new List<Node>());
            equation.Add(keys, new List<EquationData>());
            nodeData.Add(thisNode);
            EquationData rootData = new EquationData(thisNode.GetValue(), thisNode.GetIndex());
            //Check if this root node has been used elsewhere
            equationData.Add(rootData);

            //Check if has been used elsewhere, if so set a reference to the dictionary key
            CheckDuplicateNode(thisNode, keys);

            //For each child node, add it to the nodeDataList
            for (int i = 0; i < thisNode.GetChildCount(); i++)
            {
                List<Node> childNode = thisNode.GetChildNodes();
                nodeData.Add(childNode[i]);
                equationData.Add(new EquationData(childNode[i].GetValue(), childNode[i].GetIndex()));
            }
            //Set the value of dictionary of this grouping
            equationGroups[keys] = nodeData;
            equation[keys] = equationData;

            //Iterate through each childNode and repeat this process
            for (int i = 0; i < thisNode.GetChildCount(); i++)
            {
                List<Node> childNodes = thisNode.GetChildNodes();
                Node newNode = childNodes[i];
                GetEquationGroups(newNode);
            }
        }
    }

    //Begins recursively building the equations.
    private string StartEquation(int startIndex)
    {
        string equationString = "";

        equationString = BuildEquationRecursively(startIndex, equationString);
        return equationString;
    }

    //Recursively builds the equations as strings, to be stored and later evaluated
    private string BuildEquationRecursively(int startIndex, string equationString)
    {
        //If there is only 1 node in the tree
        if (equation[startIndex].Count == 1)
        {
            equationString += "(";
            equationString += equation[startIndex][0].symbol;
            equationString += ")";
        }
        //There are 2 nodes, such as a square root
        else if (equation[startIndex].Count == 2)
        {
            if (equation[startIndex][1].jumpToIndex != -1)
            {
                equationString += "(" + equation[startIndex][0].symbol + "(";
                equationString = BuildEquationRecursively(equation[startIndex][1].jumpToIndex, equationString);
                equationString += ")" + ")";
            }
            else
            {
                //Will result in "(#A)"
                equationString += "(" + equation[startIndex][0].symbol + "(";
                equationString += equation[startIndex][1].symbol;
                equationString += ")" + ")";
            }
        }
        //Standard operator such as +/*- that uses 2 elements
        else if (equation[startIndex].Count == 3)
        {
            if (equation[startIndex][1].jumpToIndex != -1)
            {
                equationString += "(";
                equationString = BuildEquationRecursively(equation[startIndex][1].jumpToIndex, equationString);
                equationString += equation[startIndex][0].symbol;
            }
            else
            {
                //Will result in "(A/..."
                equationString += "(";
                equationString += equation[startIndex][1].symbol;
                equationString += equation[startIndex][0].symbol;
            }

            if (equation[startIndex][2].jumpToIndex != -1)
            {
                string rightBranch = "";
                rightBranch = BuildEquationRecursively(equation[startIndex][2].jumpToIndex, rightBranch);
                equationString += rightBranch;
                equationString += ")";
            }
            else
            {
                equationString += equation[startIndex][2].symbol;
                equationString += ")";
            }
        }
        return equationString;
    }

    //Links all the gene equations together using one of the 4 standard operators at random.
    //only used if the 'TRAITS_ON_GENES' setting is set to false, meaning the traits will be assigned to entire chromosomes instead
    private string LinkEquations(List<string> chromosomeEquation, System.Random generator)
    {
        char[] operators = new char[4]
        {
                        '*', '/', '+', '-'
        };

        List<string> linkedEquations = new List<string>();

        for (int i = 0; i < chromosomeEquation.Count; i++)
        {
            //int operatorIndex = Random.Range(0, operators.Length);
            int operatorIndex = generator.Next(0, operators.Length);
            string equation = chromosomeEquation[i] + operators[operatorIndex] + chromosomeEquation[i + 1];
            linkedEquations.Add(equation);
            i++;
        }

        bool linkEquations = true;

        while (linkEquations)
        {
            List<string> oldList = new List<string>();
            oldList = linkedEquations;
            linkedEquations = new List<string>();
            for (int i = 0; i < oldList.Count - 1; i++)
            {
                if (oldList.Count > 1)
                {
                    // operatorIndex = Random.Range(0, operators.Length);
                    int operatorIndex = generator.Next(0, operators.Length);

                    string equation = oldList[i] + operators[operatorIndex] + oldList[i + 1];
                    linkedEquations.Add(equation);
                }
            }
            if (linkedEquations.Count <= 1)
            {
                linkEquations = false;
            }
        }
        chromosomePhenotypeChromoEquations.Add(linkedEquations[0]);
        return linkedEquations[0];
    }

    //Used to check if a node is unique (A or B) or if it is referenced elsewhere in the tree
    private bool CheckDuplicateNode(Node nodeToCheck, int thisKey)
    {
        foreach (KeyValuePair<int, List<Node>> nodeTrees in equationGroups)
        {
            for (int i = 0; i < nodeTrees.Value.Count; i++)
            {
                if (nodeToCheck.GetIndex() == nodeTrees.Value[i].GetIndex())
                {
                    int key = nodeTrees.Key;
                    EquationData updatedData = new EquationData(equation[key][i].symbol, equation[key][i].index);
                    updatedData.jumpToIndex = thisKey;
                    equation[key][i] = updatedData;
                    return true;
                }
            }
        }
        return false;
    }
}
