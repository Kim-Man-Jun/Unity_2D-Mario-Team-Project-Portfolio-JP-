using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun.Demo.Cockpit;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;
using System.IO;


public class Lobby : MonoBehaviourPunCallbacks
{
    
    [Header("�κ� INFO ���� ����")]
    [SerializeField] private Text lobby_info;
    [SerializeField] private int lobby_player_count = -1;
    [SerializeField] private int lobby_player_count_tmp; //����ȭ �� �ӽ� ����

    [Header("�� ��� ���� ����")]
    public GameObject room_btn;
    private List<RoomInfo> myList = new List<RoomInfo>();
    public RectTransform Room_List_Content;


    [Header("�� ����� ���� ����")]
    private  int max_Player;
    private Coroutine set_max_player;
    public int current_max_player;
    private Text make_room_title;

	public string mapMakeSceneName;
    

	string characterPrefab;
    float instMarioX = -6;
	// Room�� �ִ� �Լ��� �����ϱ� ���� �̺�Ʈ �Լ�
	//public UnityEvent RoomUISync;
	public PhotonView PV;
    Coroutine cor_refresh;


    //���̾�̽� ���� ���� (Ȥ�� �𸣴� png, json ���� ����)
    public List<string> file_list;
    public List<string> png_list;   
    public List<string> json_list;
    public List<string> real_json_list;
    public List<GameObject> real_object_list;
    public GameObject map_element;
    public GameObject mapSelect_Content;
    public GameObject room_obj;
    public Coroutine cor_refresh_mapData;

    private void Awake()
    {
        //Debug.Log("flag1");
        lobby_info = GameObject.Find("Lobby_info_count").GetComponent<Text>();
        Lobby_Player_Count();

		// Room_Make_Layer info init
		max_Player = 4;
        GameObject.Find("Room_Maker_Player_Scroll").GetComponent<Scrollbar>().value = 0;
        current_max_player = 1;
        make_room_title = GameObject.Find("Make_Room_Title").GetComponent<Text>();
        Room_List_Content.localPosition = new Vector3(300, Room_List_Content.localPosition.y, Room_List_Content.localPosition.z);
        GameObject.Find("Room_Maker_Player_Scroll").GetComponent<Scrollbar>().value = 0;

        // Debug.Log("flag2");

        if (cor_refresh == null)
        {
            cor_refresh = StartCoroutine(ILobby_Refresh());
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            return;
        }
        else if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            //Lobby_Init();
            // �̹� �뿡 �ִ� ���¿��� Lobby �� �ҷ��Դٸ� Room���� ��ȯ�ϱ�
            //Room_Init();
            //PV.RPC("RoomUISync", RpcTarget.AllBuffered);
            // ������ �����ǰ� �ؾ��ϴµ� ��ĳ ��

            return;
        }
        PhotonNetwork.JoinLobby();

        


		StartCoroutine(ILobby_Refresh());
    }

    private void Start()
    {
        AudioManager.instance.PlayerOneShot(MARIO_SOUND.LOBBY_BGM, true, 0);
        cor_refresh_mapData = StartCoroutine(Refresh_MapData());
    }

	private void Update()
	{
		Lobby_Player_Count();
	}


    private void Lobby_Player_Count()
	{
		lobby_player_count_tmp = PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms;
		//lobby_player_count_tmp = PhotonNetwork.CurrentRoom.PlayerCount;

		//Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);

		if (lobby_player_count_tmp != lobby_player_count)
		{
			//Debug.Log(PhotonNetwork.CurrentRoom.Name);
			lobby_player_count = lobby_player_count_tmp;
			lobby_info.text = "�κ� ����� :  " + lobby_player_count + "�� ";

			//log_text.text += "\n ���� ������Ŭ���̾�Ʈ id : " + PhotonNetwork.CurrentRoom.MasterClientId;
			//log_text.text += "\n �� �ڽ��� ������ Ŭ���̾�Ʈ ?  : " + PhotonNetwork.IsMasterClient;

			//Debug.Log(PhotonNetwork.CurrentRoom.)
		}

        if (!PhotonNetwork.IsConnected)
        {
            //GetComponent<CanvasGroup>().alpha = 0;
            //GetComponent<CanvasGroup>().interactable = false;
            SceneManager.LoadScene(0);
        }
	}

	public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("onConnectToMaster");
        PhotonNetwork.JoinLobby();
        
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
	}

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        

    }

    IEnumerator Refresh_MapData()
    {
        while (true)
        {
            if (PhotonNetwork.InRoom )
            {
                yield return new WaitForSeconds(2f);
                continue;
            }

            FirebaseDataManager tmp = new FirebaseDataManager();
            tmp.LoadStorageCustom("list2.json");
            yield return new WaitForSeconds(0.3f);
            TilemapManager TM = new TilemapManager();
            TM.JsonLoad_list();
            yield return new WaitForSeconds(0.3f);
            if (TM.map_list == null)
            {
                yield return new WaitForSeconds(1);
                continue;
            }
            file_list = TM.map_list.ToList<string>();
            png_list.Clear();
            json_list.Clear();

            Debug.Log("���ϸ���Ʈ �ٿ� �Ϸ� : " + file_list.Count);
            List<bool> down_complete = new List<bool>();

            for (int i = 0; i < file_list.Count; i++)
            {
                 tmp.LoadStorageCustom_lobby(file_list[i]);
            }

            for (int i = 0; i < file_list.Count; i++)
            {

                //id�� .png �� .json ���� ������ ������ ������ �켱 �н�
                if (file_list[i].Contains(".png"))
                {
                    png_list.Add(file_list[i]);
                }
                else if (file_list[i].Contains(".json"))
                {
                    json_list.Add(file_list[i]);
                }


            }
            yield return new WaitForSeconds(3f);

            Debug.Log("�ʵ� �Ϸ� png ���� :  " + png_list.Count + " .json ����  " + json_list.Count + " << ");
            RectTransform[] rects = mapSelect_Content.transform.GetComponentsInChildren<RectTransform>();
            for (int i = 0; i < rects.Length; i++)
            {
                    Debug.Log("destroy for ��");
                if (rects[i].gameObject != mapSelect_Content.gameObject)
                {
                    Debug.Log("destroy");
                    Destroy(rects[i].gameObject);
                }
            }
            room_obj.GetComponent<Room>().maxMapNum = 0;
            for (int i = 0; i < png_list.Count; i++)
            {
                Debug.Log("s flag 1");
                string tmp_s = png_list[i].Replace(".png", ".json");
                string path = Application.dataPath + "/" + png_list[i];
                string path2 = Application.dataPath + "/" + png_list[i];
                //string path2 = Path.Combine(Application.dataPath, png_list[i]);
                if (File.Exists(path2))
                {
                    if (json_list.Contains(tmp_s))
                    {
                        Debug.Log("real .. + " + tmp_s);
                        real_json_list.Add(tmp_s);
                        

                        room_obj.GetComponent<Room>().maxMapNum++;
                        var a = Instantiate(map_element, transform.position, Quaternion.identity);
                        a.transform.parent = mapSelect_Content.transform;
                        real_object_list.Add(a);
                        byte[] byteTexture = System.IO.File.ReadAllBytes(path);

                        Texture2D texture = new Texture2D(2, 2);

                        texture.LoadImage(byteTexture);



                        Rect rect = new Rect(0, 0, texture.width, texture.height);

                        //a.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                        a.GetComponent<Image>().sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                        a.transform.localScale = new Vector3(1, 1, 1);
                        a.transform.localPosition = new Vector3(a.transform.localPosition.x, a.transform.localPosition.y, 0);
                    }
                }
                else
                {
                    Debug.Log("���� ���� X" + path2 + " <<<<<<<<<" + path);
                }
                
            }


            yield return new WaitForSeconds(5f);
        }
        
    }

	public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        
    }


    
    // �κ� �ִ� ����� ���� �� ����
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        
        //Debug.Log("OnRoomListUpdate ������Ʈ..");
        base.OnRoomListUpdate(roomList);
        int updated_count = roomList.Count;
        for(int i = 0; i < updated_count; i++)
        {
            if (roomList[i].RemovedFromList)
            {
                if (myList.Contains(roomList[i])){
                    myList.Remove(roomList[i]);
                }
            } else
            {
                if (myList.Contains(roomList[i]))
                {
                    myList[myList.IndexOf(roomList[i])] = roomList[i];
                }
                else
                {
                    myList.Add(roomList[i]);
                }
            }
        }
        
        Room_List_Init();
    }
    
    private void Room_List_Init()
    {
		RectTransform[] tmp = Room_List_Content.GetComponentsInChildren<RectTransform>();
        
        if (tmp != null)
        {
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i].gameObject != Room_List_Content.gameObject)
                    Destroy(tmp[i].gameObject);
                    //tmp[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < myList.Count; i++)
        {
            //Debug.Log("���� ���� ����" + myList.Count);
			//Debug.Log(myList[i].CustomProperties["room_name"].ToString() + " ���� (bool)myList[i].CustomProperties[\"editor\"]" + (bool)myList[i].CustomProperties["editor"]);
			//Debug.Log(myList[i].CustomProperties["room_name"].ToString() + " �ش� ���� myList[i].CustomProperties.ContainsKey(\"editor\"): " + myList[i].CustomProperties.ContainsKey("editor"));
			if ((bool)myList[i].CustomProperties["editor"]) continue;


			var a = Instantiate(room_btn, Vector3.zero, Quaternion.identity);
            a.transform.parent = Room_List_Content;
            a.transform.localScale = new Vector3(1, 1, 1);
            
            a.GetComponent<Lobby_Room_Btn>().my_room_info = myList[i];
            a.GetComponent<Lobby_Room_Btn>().room_num = i + 1;
            a.GetComponent<Lobby_Room_Btn>().master_client_id = myList[i].masterClientId;
            //Debug.Log(myList[i].CustomProperties.ContainsKey("mapMakingRoom"));
            
            // At Room_List_Init(), Turned on/off Playing Text according to "roomstate"
            // +interactive(roomState)
            a.GetComponent<Button>().interactable = !((bool)myList[i].CustomProperties["room_state"]);
			a.GetComponent<Lobby_Room_Btn>().room_start_state.gameObject.SetActive((bool)myList[i].CustomProperties["room_state"]);
            // false�� interactable true���� ��
            
            a.GetComponent<Lobby_Room_Btn>().room_master_name = myList[i].CustomProperties["master_name"].ToString();
            a.GetComponent<Lobby_Room_Btn>().room_name = myList[i].CustomProperties["room_name"].ToString();
            
        }
    }
    // Btn �� ����� Ŭ��
    public void Map_Make_Click()
    {
        SceneManager.LoadScene(mapMakeSceneName);
    }

	// Btn �� ����� Ŭ��
	public void Room_Plus_Click()
    {
        //var a = GameObject.Find("Lobby_Layer");
        //a.transform.Translate(new Vector3(-100, 0, 0));
        //StartCoroutine(CorLerp(a, a.transform.position, new Vector3(-2000, a.transform.position.y, a.transform.position.z)));

        var a = GameObject.Find("Room_Make_Layer");

        // If you leave the room and click Create, the title of the room you created before is written
        // So we need to initialize the room title inputfield
        a.GetComponentInChildren<InputField>().text = "";

		//a.transform.localPosition = new Vector3(0, 1000, 0);
		StartCoroutine(CorLerp(a,new Vector3(0,1000,0), new Vector3(0,-100,0)));
        
        Debug.Log("room plus click");
    }

	// Btn �� ����� ���̾� ���� �� â�ݱ� ��ư(x)
	public void Room_Close_Click()
    {
       
        var a = GameObject.Find("Room_Make_Layer");
        StartCoroutine(CorLerp(a, a.GetComponent<RectTransform>().localPosition, new Vector3(0, 1300, 0)));

    }

	// Btn �ִ� �÷��̾� �ø���
	public void Room_Maker_Player_Select_Plus()
	{

		if (current_max_player + 1 <= max_Player)
		{
			if (set_max_player != null)
				StopCoroutine(set_max_player);
			current_max_player++;
			set_max_player = StartCoroutine(Cor_Room_Maker_Player_Scroll());
		}
	}

	// Btn �ִ� �÷��̾� ���̱�
	public void Room_Maker_Player_Select_Minus()
	{

		if (current_max_player - 1 <= max_Player && current_max_player - 1 > 0)
		{
			if (set_max_player != null)
				StopCoroutine(set_max_player);
			current_max_player--;
			set_max_player = StartCoroutine(Cor_Room_Maker_Player_Scroll());

		}
	}
    
    // Btn �� ���� Ŭ��
    public void Try_Room_Make()
    {
        string _title = make_room_title.text;
        int _max_player = current_max_player;
        bool _isRoomStart = false;
        //bool _isMapMakingRoom = false;
        Debug.Log(_title + " " + _max_player);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = _max_player;

		//options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable(){{"master_name", PhotonNetwork.NickName},{"room_name", _title}};
		options.CustomRoomProperties = new Hashtable() { { "master_name", WIndowManager.instance.nickName }, { "room_name", _title }, { "room_state", _isRoomStart }, { "editor", false } , { "map_num", " "} };

		//options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {  };
		options.CustomRoomPropertiesForLobby = new string[] { "master_name", "room_name", "room_state", "editor", "map_num" };


		bool make_success = PhotonNetwork.JoinOrCreateRoom(_title, options, null);
        if (make_success)
        {
            Debug.Log("�� ���� ����");
        }
        else
        {
            Debug.Log("�� ���� ����");
            // �� ���� ���� UI ����
        }

    }

	// UI������ �����̱�
	IEnumerator CorLerp(GameObject gameObject, Vector3 start_pos, Vector3 des_pos)
    {
        gameObject.SetActive(true);
        RectTransform RT = gameObject.GetComponent<RectTransform>();
        RT.localPosition = start_pos;
        while (Vector3.Distance(RT.localPosition, des_pos) > 50f) 
        {
            //Debug.Log(Vector3.Distance(RT.localPosition, des_pos));
            RT.localPosition = Vector3.Lerp(RT.localPosition, des_pos, 0.3f);
            //yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(0.02f);
        }
       
        yield break;
    }

    // �ִ� �÷��̾� ������ �� �߾��� ���ڰ� ������ �� �Ѿ�� �ϴ� ��
    IEnumerator Cor_Room_Maker_Player_Scroll()
    {
        var a = GameObject.Find("Room_Maker_Player_Scroll").GetComponent<Scrollbar>();
        float current_value = a.value;
        float des_value =  1.0f/(max_Player-1) * (current_max_player-1);

        //Debug.Log(current_value + " " + des_value + " " + max_Player + "  " );
        while (true)
        {
            a.value = (a.value + des_value) / 2;
            if (Mathf.Abs(des_value - a.value) < 0.001f)
            {
                break;
            }
            yield return null;
        }
        yield break;;
    }
    

	public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        base.OnFriendListUpdate(friendList);
    }

    public void Lobby_Init()
    {
        GameObject lobby = GameObject.Find("Lobby_Layer");
        StartCoroutine(CorLerp(lobby, new Vector3(1100,0,0), new Vector3(-0,0,0)));
        // room move leftside
        GameObject room_layer = GameObject.Find("Room_Layer");
        StartCoroutine(CorLerp(room_layer, new Vector3(0, 0, 0), new Vector3(1100, 0, 0)));
        // room maker move upside
        var a = GameObject.Find("Room_Make_Layer");
        Vector3 localPositionA = a.GetComponent<RectTransform>().localPosition;
        StartCoroutine(CorLerp(a, localPositionA, new Vector3(localPositionA.x, 1000, 0)));
    }

	public void Room_Init()
	{
		// lobby move leftside
		GameObject lobby = GameObject.Find("Lobby_Layer");
		StartCoroutine(CorLerp(lobby, lobby.GetComponent<RectTransform>().localPosition,
			lobby.GetComponent<RectTransform>().localPosition + new Vector3(-2000, 0, 0)));
		// room move leftside
		GameObject room_layer = GameObject.Find("Room_Layer");
		StartCoroutine(CorLerp(room_layer, new Vector3(0, 1100, 0), new Vector3(-30, 0, 0)));
        // room maker move upside
		var a = GameObject.Find("Room_Make_Layer");
		Vector3 localPositionA = a.GetComponent<RectTransform>().localPosition;
		StartCoroutine(CorLerp(a, localPositionA, new Vector3(localPositionA.x, 1000, 0)));
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
        


        Room_Init();
        
		// The number of players in the room determines which prefab is created.
		int playerNum = PhotonNetwork.CurrentRoom.PlayerCount;
		Vector3 spawnPosition = new Vector3(instMarioX, 2, 0);
		switch (playerNum)
        {
            case 1:
				characterPrefab = "Prefabs/Mario";
				break;
            case 2:
				characterPrefab = "Prefabs/Mario2";
                break;
            case 3:
				characterPrefab = "Prefabs/Mario3";
                break;
            case 4:
				characterPrefab = "Prefabs/Mario4";
                break;
        }
		PhotonNetwork.Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
		instMarioX++;
		if (instMarioX > -2) instMarioX = -6;

		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
		customProperties.Add("characterName", characterPrefab);
		PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

		//Debug.Log("marioX ��ġ �� �� �ٲ�� �ž�: " + new Vector3(instMarioX, 2, 0));
		// Mario spawns from x values ??0, 1, 2, 3

		PV.RPC("RoomUISync", RpcTarget.AllBuffered);
		//RoomUISync.Invoke();
	}

    


    public override void OnLeftRoom()
	{
        
		base.OnLeftRoom();
        // �� ���������� ����
        GameObject room = GameObject.Find("Room_Layer");
        Vector3 tmp = new Vector3(-1960, 0, 0);
        Vector3 tmp2 = new Vector3(-28, 44, 0);
        StartCoroutine(CorLerp(room, tmp2,
           tmp2 + new Vector3(2000, 0, 0)));

        // �κ� �����;� ��
        GameObject lobby = GameObject.Find("Lobby_Layer");
        StartCoroutine(CorLerp(lobby, tmp,
           tmp + new Vector3(2000, 0, 0)));

        // ���� ������ �����ͷ� ���ϱ� �κ�� �ٽ� ������ �ؾ� ��
        PhotonNetwork.JoinLobby();

	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
        // ������ ������ UI ����ȭ
		PV.RPC("RoomUISync", RpcTarget.AllBuffered);
	}

    // �κ� ���ΰ�ħ 1�ʸ��� �ڷ�ƾ
    public IEnumerator ILobby_Refresh()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            //Debug.Log("refresh..." + PhotonNetwork.IsConnected + " " + PhotonNetwork.IsConnectedAndReady + " " + PhotonNetwork.InLobby + " " + PhotonNetwork.InRoom);
            Lobby_Refresh();

        }
    }

    // �κ� ������
    public void Lobby_Refresh()
    {
        //PhotonNetwork.JoinLobby();

        //RoomOptions RO = new RoomOptions();
        //RO.IsVisible = false;
        //RO.MaxPlayers = 30;
        if(PhotonNetwork.InLobby)
            PhotonNetwork.LeaveLobby();
        

    }

    // �κ� ������ �� �ٷ� �ٽ� ������ �����
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        //Debug.Log("lobby left");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log(cause + " " + cause.ToString());
        PhotonNetwork.ConnectUsingSettings();
    }
}