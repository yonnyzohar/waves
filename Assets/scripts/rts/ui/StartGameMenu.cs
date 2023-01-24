namespace RTS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StartGameMenu : Engine.Actor
    {
        GameObject mc;
        Button btn;
        // Start is called before the first frame update
        public StartGameMenu(RTSContext context):base(context)
        {
            mc = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/IntroMenu"));
            int b = 1;
            btn = mc.transform.Find("EnterBtn").GetComponent<Button>();

            btn.onClick.AddListener(onEnterClicked);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void onEnterClicked()
        {
            btn.onClick.RemoveListener(onEnterClicked);
            Debug.Log("YEY!!");
            GameObject.Destroy(mc);
            dispatchEvent("MENU_REMOVED");
        }
    }

}
