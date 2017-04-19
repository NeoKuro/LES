using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job : ThreadedJob
{
    public CreatureManager hManager;

    protected override void ThreadFunction()
    {

        hManager.genomeManager.GenerateNewGenome(hManager);

        hManager.phenotypeNodesManager.GeneratePhenotype(hManager.genomeManager.genome);
        hManager.phenotypeNodesManager.GeneratePhenotypeEquation();
        hManager.phenotypeNodesManager.EvaluatePhenotypeEquations(hManager);

        //Performed separately so that the trait layout may be assigned depending on settings, then evaluated
        hManager.traitManager.GenerateTraitsList(hManager, hManager.speciesName);
        hManager.traitManager.EvaluateTraits(hManager, hManager.speciesName);
    }

    protected override void OnFinished()
    {
        hManager.loadCreatureComplete = true;
    }

}
