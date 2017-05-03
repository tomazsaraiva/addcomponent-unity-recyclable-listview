using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddComponent;

public class GalleryControllerTest : MonoBehaviour 
{
	[SerializeField]
	private List _list;

	[SerializeField]
	private ListItemBase _listItem;

	[SerializeField]
	private Sprite[] _sprites;


	void Start()
	{
		_list.onItemLoaded = HandleOnItemLoadedHandler;

		_list.Create (_sprites.Length, _listItem);
		_list.gameObject.SetActive (true);
	}

	void HandleOnItemLoadedHandler (ListItemBase item)
	{
		ListItemMovie movieItem = (ListItemMovie)item;
		movieItem.SetImage(_sprites[item.Index]);
	}
}