using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using UnityEngine.UI;
using System.IO;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEditor;

public class FirebaseDataManager
{

    FirebaseStorage storage;
    StorageReference storageReference;

    public Text saveQutNum;
    public Text loadQutNum;

    //��ư �׼� ���� (���� ��ζ� �����̸����� �־��ּ���)
    public void SaveStorageCustom(string file_path)
    {
        //���� ���
        string path = Application.dataPath + "/" + file_path;
        //storage ����Ʈ �ν��Ͻ� ����
        storage = FirebaseStorage.DefaultInstance;
        //storageReferene�� ���̾�̽� URL �ּ� ����
        storageReference = storage.GetReferenceFromUrl("gs://teamproject-supermariomaker.appspot.com");
        //byte[] bytes = File.ReadAllBytes(path);
        StorageReference uploadRef = storageReference.Child("Maps/" + file_path);
        uploadRef.PutFileAsync(path).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("����333" + file_path + " " + task.Exception.ToString());
            }
            else
            {
                Debug.Log("����333");
            }
        });
    }

    public void LoadStorageCustom(string file_path)
    {
        Debug.Log("load list...");
        string path = Application.dataPath + "/" + file_path;
        storage = FirebaseStorage.DefaultInstance;
        StorageReference reference = storage.GetReferenceFromUrl
            ("gs://teamproject-supermariomaker.appspot.com");
        StorageReference downloadRef = reference.Child("Maps/" + file_path);

        downloadRef.GetFileAsync(path).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                //loadQutNum.text = "����";
                Debug.Log(path + " ���̾�̽����� list �������� ����'");
            }
            else
            {
                //loadQutNum.text = "����";
                Debug.Log(path + " ���̾�̽����� list �������� ����'");
            }
        });
    }

    public bool LoadStorageCustom_lobby(string file_path)
    {
        //Debug.Log("load list...");
        string path = Application.dataPath + "/" + file_path;
        storage = FirebaseStorage.DefaultInstance;
        StorageReference reference = storage.GetReferenceFromUrl
            ("gs://teamproject-supermariomaker.appspot.com");
        StorageReference downloadRef = reference.Child("Maps/" + file_path);
        bool val = false;

        downloadRef.GetFileAsync(path).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                //loadQutNum.text = "����";
                //Debug.Log(path + " ���̾�̽����� list �������� ���� lobby'");
                val = true;
            }
            else
            {
                //loadQutNum.text = "����";
                //Debug.Log(path + " ���̾�̽����� list �������� ���� lobby'");
            }
        });

        return val;
    }

    //��ư �׼� ����
    public void SaveStorage()
    {
        //���� ���
        string path = Application.dataPath + "/Kdm/Scenes/KdmScene.unity";
        //storage ����Ʈ �ν��Ͻ� ����
        storage = FirebaseStorage.DefaultInstance;
        //storageReferene�� ���̾�̽� URL �ּ� ����
        storageReference = storage.GetReferenceFromUrl("gs://teamproject-supermariomaker.appspot.com");
        //byte[] bytes = File.ReadAllBytes(path);
        StorageReference uploadRef = storageReference.Child("Maps/KdmScene.unity");
        saveQutNum.text = "������";
        uploadRef.PutFileAsync(path).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                saveQutNum.text = "����111";
            }
            else
            {
                Debug.Log("����");
                saveQutNum.text = "����111";
            }
        });

    }



    public void LoadStorage()
    {
        string path = Application.dataPath + "/KMJ/Mapdownload/KdmScene.unity";
        storage = FirebaseStorage.DefaultInstance;
        StorageReference reference = storage.GetReferenceFromUrl
            ("gs://teamproject-supermariomaker.appspot.com");
        StorageReference downloadRef = reference.Child("Maps");

        downloadRef.GetFileAsync(path).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                loadQutNum.text = "����222";
                Debug.Log(path);
            }
            else
            {
                loadQutNum.text = "����222";
            }
        });
    }
}

//1. ����Ƽ ��⿡�� IOS�� �ٿ�ε�
//2. Firebase_unity_sdk ���̾�̽� ���Ļ���Ʈ���� �ٿ�ε�
//3. ����Ƽ���� FirebaseStorage.unitypackage import
//4. ���̾�̽� ���� ����Ʈ���� ����ҷ� ���� ���� ���� ������Ʈ ���� - ������ư
// - google-services.json �ٿ�ε� - ����Ƽ Plugins�� ����ֱ� 
