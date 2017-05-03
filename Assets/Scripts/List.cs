using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AddComponent
{
	public class List : MonoBehaviour 
	{
		#region HANDLER ItemLoaded

		public delegate void OnItemLoadedHandler (ListItemBase item);

		public OnItemLoadedHandler onItemLoaded;

		public void ItemLoaded(ListItemBase item, bool clear = false)
		{
			if (onItemLoaded != null)
			{
				onItemLoaded(item);

				if (clear)
				{
					onItemLoaded = null;
				}
			}
		}

		#endregion

		#region HANDLER ItemSelected

		public delegate void OnItemSelectedHandler (ListItemBase item);

		public OnItemSelectedHandler onItemSelected;

		public void ItemSelected(ListItemBase item, bool clear = false)
		{
			if (onItemSelected != null)
			{
				onItemSelected(item);

				if (clear)
				{
					onItemSelected = null;
				}
			}
		}

		#endregion


		private enum ScrollOrientation
		{
			HORIZONTAL,
			VERTICAL
		}

		private enum ScrollDirection
		{
			NEXT,
			PREVIOUS
		}


		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _viewport;

		[SerializeField]
		private RectTransform _content;

		[SerializeField]
		private ScrollOrientation _scrollOrientation;

		[SerializeField]
		private float _spacing;


		private List<ListItemBase> _itemsList;

		private float _itemSize;
		private float _lastPosition;

		private int _itemsTotal;
		private int _itemsVisible;

		private int _itemsToRecycleBefore;
		private int _itemsToRecycleAfter;

		private int _currentIndex;


		public void Create(int items, ListItemBase listItemPrefab)
		{
			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				_scrollRect.vertical = false;
				_scrollRect.horizontal = true;

				_content.anchorMin = new Vector2 (0, 0);
				_content.anchorMax = new Vector2 (0, 1);

				_itemSize = listItemPrefab.Size.x;
				_content.sizeDelta = new Vector2(_itemSize * items + _spacing * (items - 1), 0);
				break;

			case ScrollOrientation.VERTICAL:
				_scrollRect.vertical = true;
				_scrollRect.horizontal = false;

				_content.anchorMin = new Vector2 (0, 1);
				_content.anchorMax = new Vector2 (1, 1);

				_itemSize = listItemPrefab.Size.y;
				_content.sizeDelta = new Vector2(0, _itemSize * items + _spacing * (items - 1));
				break;
			}


			_itemsVisible = Mathf.CeilToInt (GetViewportSize () / _itemSize);

			int itemsToInstantiate = _itemsVisible;

			if (_itemsVisible == 1)
			{
				itemsToInstantiate = 5;
			}
			else if (itemsToInstantiate < items)
			{
				itemsToInstantiate *= 2;
			}

			if(itemsToInstantiate > items)
			{
				itemsToInstantiate = items;
			}

			_itemsList = new List<ListItemBase> ();

			for (int i = 0; i < itemsToInstantiate; i++)
			{
				ListItemBase item = CreateNewItem (listItemPrefab, i, _itemSize);
				item.onSelected = HandleOnSelectedHandler;
				item.Index = i;

				_itemsList.Add(item);

				ItemLoaded (item);
			}
				
			_itemsTotal = items;

			_currentIndex = _itemsList.Count - 1;

			_itemsToRecycleAfter = _itemsList.Count - _itemsVisible;


			_scrollRect.onValueChanged.AddListener ((Vector2 position) => 
			{
				Recycle();
			});	
		}

		public void Destroy()
		{
			_scrollRect.verticalNormalizedPosition = 1;

			if(_itemsList != null)
			{
				for (int i = 0; i < _itemsList.Count; i++)
				{
					Destroy (_itemsList [i].gameObject);
				}

				_itemsList.Clear ();
				_itemsList = null;
			}

			_lastPosition = -1;
		}


		void HandleOnSelectedHandler (ListItemBase item)
		{
			ItemSelected (item);
		}
			
			
		private ListItemBase CreateNewItem(ListItemBase prefab, int index, float dimension)
		{
			GameObject instance = (GameObject)Instantiate (prefab.gameObject, Vector3.zero, Quaternion.identity);
			instance.transform.SetParent (_content.transform);
			instance.transform.localScale = Vector3.one;
			instance.SetActive (true);

			float position = index * (dimension + _spacing) + dimension / 2;

			RectTransform rectTransform = instance.GetComponent <RectTransform> ();

			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				rectTransform.anchorMin = new Vector2 (0, 0);
				rectTransform.anchorMax = new Vector2 (0, 1);
				rectTransform.anchoredPosition = new Vector2 (position, 0);
				rectTransform.offsetMin = new Vector2 (rectTransform.offsetMin.x, 0);
				rectTransform.offsetMax = new Vector2 (rectTransform.offsetMax.x, 0);
				break;

			case ScrollOrientation.VERTICAL:
				rectTransform.anchorMin = new Vector2 (0, 1);
				rectTransform.anchorMax = new Vector2 (1, 1);
				rectTransform.anchoredPosition = new Vector2 (0, -position);
				rectTransform.offsetMin = new Vector2 (0, rectTransform.offsetMin.y);
				rectTransform.offsetMax = new Vector2 (0, rectTransform.offsetMax.y);
				break;
			}

			return instance.GetComponent <ListItemBase> ();
		}


		private void Recycle()
		{
			if(_lastPosition == -1)
			{
				_lastPosition = GetContentPosition ();

				return;
			}

			int displacedRows = Mathf.FloorToInt (Mathf.Abs (GetContentPosition() - _lastPosition) / _itemSize);

			if (displacedRows == 0)
			{
				return;
			}

			ScrollDirection direction = GetScrollDirection ();

			for (int i = 0; i < displacedRows; i++)
			{
				switch (direction)
				{
				case ScrollDirection.NEXT:
					
					if (_itemsToRecycleBefore >= (_itemsList.Count - _itemsVisible) / 2 && _currentIndex < _itemsTotal - 1)
					{
						_currentIndex++;

						RecycleItem (direction);
					}
					else
					{
						_itemsToRecycleBefore++; 
						_itemsToRecycleAfter--;
					}

					break;

				case ScrollDirection.PREVIOUS:

					if (_itemsToRecycleAfter >= (_itemsList.Count - _itemsVisible) / 2 && _currentIndex > _itemsList.Count - 1)
					{
						RecycleItem (direction);

						_currentIndex--;
					}
					else
					{
						_itemsToRecycleBefore--; 
						_itemsToRecycleAfter++;
					}

					break;
				}

				if ((direction == ScrollDirection.NEXT && _scrollOrientation == ScrollOrientation.VERTICAL) ||
				    (direction == ScrollDirection.PREVIOUS && _scrollOrientation == ScrollOrientation.HORIZONTAL))
				{
					_lastPosition += _itemSize + _spacing;
				}
				else
				{
					_lastPosition -= _itemSize + _spacing;
				}

				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.Log("_itemsToRecycleBefore: " + _itemsToRecycleBefore);
				#endif

				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.Log("_itemsToRecycleAfter: " + _itemsToRecycleAfter);
				#endif
			}
		}

		private void RecycleItem(ScrollDirection direction)
		{
			ListItemBase firstItem = _itemsList [0];
			ListItemBase lastItem = _itemsList [_itemsList.Count - 1];

			float targetPosition = (_itemSize + _spacing);

			switch(direction)
			{
			case ScrollDirection.NEXT:

				switch (_scrollOrientation)
				{
				case ScrollOrientation.HORIZONTAL:
					firstItem.Position = new Vector2 (lastItem.Position.x + targetPosition, firstItem.Position.y);
					break;

				case ScrollOrientation.VERTICAL:
					firstItem.Position = new Vector2 (firstItem.Position.x, lastItem.Position.y - targetPosition);
					break;
				}

				firstItem.Index = _currentIndex;
				firstItem.transform.SetAsLastSibling ();

				_itemsList.RemoveAt (0);
				_itemsList.Add (firstItem);

				ItemLoaded (firstItem);
				break;

			case ScrollDirection.PREVIOUS:

				switch (_scrollOrientation)
				{
				case ScrollOrientation.HORIZONTAL:
					lastItem.Position = new Vector2 (firstItem.Position.x - targetPosition, lastItem.Position.y);
					break;

				case ScrollOrientation.VERTICAL:
					lastItem.Position = new Vector2 (lastItem.Position.x, firstItem.Position.y + targetPosition);
					break;
				}

				lastItem.Index = _currentIndex - _itemsList.Count;
				lastItem.transform.SetAsFirstSibling ();

				_itemsList.RemoveAt (_itemsList.Count - 1);
				_itemsList.Insert (0, lastItem);

				ItemLoaded (lastItem);
				break;
			}
		}


		private float GetContentPosition()
		{
			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				return _content.anchoredPosition.x;

			case ScrollOrientation.VERTICAL:
				return _content.anchoredPosition.y;

			default:
				return 0;
			}
		}

		private float GetViewportSize()
		{
			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				return _viewport.rect.width;

			case ScrollOrientation.VERTICAL:
				return _viewport.rect.height;

			default:
				return 0;
			}
		}

		private ScrollDirection GetScrollDirection()
		{
			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				return _lastPosition < GetContentPosition() ? ScrollDirection.PREVIOUS : ScrollDirection.NEXT;

			case ScrollOrientation.VERTICAL:
				return _lastPosition > GetContentPosition() ? ScrollDirection.PREVIOUS : ScrollDirection.NEXT;

			default:
				return ScrollDirection.NEXT;
			}
		}
	}
}