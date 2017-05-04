using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddComponent;
using System.IO;

public class GalleryControllerTest : MonoBehaviour 
{
	[SerializeField]
	private List _list;

	[SerializeField]
	private ListItemBase _listItem;


	void Start()
	{
		_list.onItemLoaded = HandleOnItemLoadedHandler;

		_list.Create (10, _listItem);
		_list.gameObject.SetActive (true);
	}

	void HandleOnItemLoadedHandler (ListItemBase item)
	{
		((ListItemMovie)item).SetImage(Resources.Load<Sprite>("posters/" + item.Index));
	}
}