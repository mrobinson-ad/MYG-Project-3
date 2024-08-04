using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

namespace FlowerProject
{   
    /// <summary>
    /// Class for the tiles that make up the board with a reference to an Item Scriptable Object that makes up the content of the tile
    /// </summary>
    public class Tile : MonoBehaviour
    {
        public int x;
        public int y;

        public bool isMatched = false;

        [SerializeField] private Item _item;

        public Item Item
        {
            get => _item;

            set
            {
                if (_item == value) return;

                _item = value;

                icon.sprite = _item.sprite;
            }
        }

        [SerializeField] private bool _isSelected = false;

        public bool IsSelected // When the tile is selected this is set to true and the scale of the icon increases
        {
            get => _isSelected;

            set
            {
                if (value == true)
                {
                    _isSelected = value;
                    icon.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.4f);
                }
                else
                {
                    _isSelected = value;
                    icon.transform.DOScale(Vector3.one, 0.2f);
                }
            }
        }

        public Image icon;

        public Button button;


        private void Start() => button.onClick.AddListener(() => Board.Instance.Select(this)); // Subscribe to the Select method from the Board class

    }
}