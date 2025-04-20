using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateInformation : Singleton<UpdateInformation>
{
    GameObject informationPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnInformationPanel()
    {
        informationPanel.SetActive(true);
    }
}
