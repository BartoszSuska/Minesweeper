using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Com.Boarshroom.Minesweeper
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] GameObject squarePrefab;
        [SerializeField] float lineLength;
        [SerializeField] int bombsNumber;
        [SerializeField] List<Transform> squares = new List<Transform>();
        [SerializeField] List<Transform> bombsPos = new List<Transform>();
        [SerializeField] List<Transform> blanksPos = new List<Transform>();
        Square squareScript;
        [SerializeField] Transform background;
        [SerializeField] Transform backgroundBorder;
        [SerializeField] GameObject bombPrefab;
        [SerializeField] GameObject[] numbersPrefabs;
        bool firstClick;
        int lockedCounter = 0;
        [SerializeField] TMP_Text lockedCounterText;
        bool gameActive;
        [SerializeField] GameObject endScreen;
        [SerializeField] TMP_Text endScreenText;
        [SerializeField] TMP_Text scoreCounterText;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text highScoreText;
        float timeCounter = 0f;
        int scoreCounter = 0;
        [SerializeField] List<Transform> clickedSquares = new List<Transform>();

        private void Start()
        {
            bombsNumber = PlayerPrefs.GetInt("Bombs");
            lineLength = PlayerPrefs.GetFloat("Line");

            float lineLengthHalf = lineLength / 2;
            MakeMap(lineLengthHalf);
            firstClick = true;
            gameActive = true;

            scoreCounter = squares.Count - bombsNumber;
        }

        private void Update()
        {
            if(gameActive)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    RaycastHit2D raycastHit = Physics2D.Raycast(mousePos, Vector2.zero);
                    if (raycastHit.collider != null)
                    {
                        squareScript = raycastHit.collider.GetComponent<Square>();

                        if (!squareScript.locked)
                        {
                            if (ClickOnBomb(raycastHit.collider.transform.position))
                            {
                                gameActive = false;
                                LoseScreen();
                            }
                            else
                            {
                                clickedSquares.Add(raycastHit.collider.transform);
                            }

                            if (firstClick)
                            {
                                PlaceBombs(raycastHit.collider.transform.position);
                                PlaceNumbers();
                                firstClick = !firstClick;

                                lockedCounter = bombsNumber;
                                lockedCounterText.text = lockedCounter.ToString();
                            }

                            CheckBlanks(raycastHit.collider.transform.position);

                            Destroy(raycastHit.collider.gameObject);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    RaycastHit2D raycastHit = Physics2D.Raycast(mousePos, Vector2.zero);
                    if (raycastHit.collider != null)
                    {
                        squareScript = raycastHit.collider.GetComponent<Square>();
                        lockedCounter += squareScript.SetFlag();
                        lockedCounterText.text = lockedCounter.ToString();
                    }
                }

                if(!firstClick)
                {
                    timeCounter += Time.deltaTime;
                    scoreCounterText.text = Mathf.RoundToInt(timeCounter).ToString();
                }

                if(clickedSquares.Count == squares.Count)
                {
                    WinScreen();
                }
            }
        }

        bool ClickOnBomb(Vector2 clickPos)
        {
            foreach(Transform bombTransform in bombsPos)
            {
                Vector2 bombPos = bombTransform.position;

                if(bombPos == clickPos)
                {
                    return true;
                }
            }

            return false;
        }

        void MakeMap(float length)
        {
            for(float i = -length; i <= length; i+=0.5f)
            {
                for(float j = -length; j <= length; j+=0.5f)
                {
                    Vector2 squarePos = new Vector2(i, j);
                    GameObject square = Instantiate(squarePrefab, squarePos, Quaternion.identity);
                    squares.Add(square.transform);
                }
            }

            background.localScale = new Vector2(lineLength + 0.5f, lineLength + 0.5f);
            backgroundBorder.localScale = new Vector2(lineLength + 1f, lineLength + 1f);
        }

        void PlaceBombs(Vector2 clickPos)
        {
            for(int i = 0; i < bombsNumber; i++)
            {
                Vector2 bombPos;
                int squareNumber = 0;

                do
                {                    
                    squareNumber = Random.Range(0, squares.Count);
                    bombPos = squares[squareNumber].position;
                } while (bombPos == clickPos || FirstClickAround(clickPos, bombPos));
                FirstClickAround(clickPos, bombPos);
                GameObject bomb = Instantiate(bombPrefab, bombPos, Quaternion.identity);
                bombsPos.Add(bomb.transform);
                squares.Remove(squares[squareNumber]);
            }
        }

        bool FirstClickAround(Vector2 clickPos, Vector2 bombPos)
        {
            int places = 0;

            places += CheckSquare(clickPos, bombPos, 0.5f, 0f);
            places += CheckSquare(clickPos, bombPos, 0f, 0.5f);
            places += CheckSquare(clickPos, bombPos, -0.5f, 0f);
            places += CheckSquare(clickPos, bombPos, 0f, -0.5f);
            places += CheckSquare(clickPos, bombPos, 0.5f, 0.5f);
            places += CheckSquare(clickPos, bombPos, -0.5f, -0.5f);
            places += CheckSquare(clickPos, bombPos, 0.5f, -0.5f);
            places += CheckSquare(clickPos, bombPos, -0.5f, 0.5f);

            if (places > 0)
            {
                //Debug.Log(places);
                return true;
            }
            //Debug.Log("falsz");
            return false;
        }

        int CheckSquare(Vector2 clickPos, Vector2 bombPos, float x, float y)
        {
            Vector2 targetPos = new Vector2(bombPos.x + x, bombPos.y + y);

            if(targetPos == clickPos)
            {
                return 1;
            }

            //for(int i = 0; i < squares.Count; i++)
            //{
            //    Vector2 squarePos = squares[i].position;
                
            //    if(squarePos == targetPos)
            //    {
            //        return 1;
            //    }
            //}

            return 0;
        }

        void PlaceNumbers()
        {
            foreach(Transform transform in squares)
            {
                int bombs = CheckBombs(transform.position);
                Instantiate(numbersPrefabs[bombs], transform.position, Quaternion.identity);

                if(bombs == 0)
                {
                    blanksPos.Add(transform);
                }
            }
        }

        int CheckBombs(Vector2 pos)
        {
            int bombs = 0;

            foreach(Transform bombPos in bombsPos)
            {
                bombs += CheckBomb(bombPos.position, pos, 0.5f, 0f);
                bombs += CheckBomb(bombPos.position, pos, 0f, 0.5f);
                bombs += CheckBomb(bombPos.position, pos, -0.5f, 0f);
                bombs += CheckBomb(bombPos.position, pos, 0f, -0.5f);
                bombs += CheckBomb(bombPos.position, pos, 0.5f, 0.5f);
                bombs += CheckBomb(bombPos.position, pos, -0.5f, -0.5f);
                bombs += CheckBomb(bombPos.position, pos, 0.5f, -0.5f);
                bombs += CheckBomb(bombPos.position, pos, -0.5f, 0.5f);
            }

            return bombs;
        }

        int CheckBomb(Vector2 bombPos, Vector2 targetPos, float x, float y)
        {
            Vector2 pos = new Vector2(targetPos.x + x, targetPos.y + y);

            if(bombPos == pos)
            {
                return 1;
            }

            return 0;
        }

        void CheckBlanks(Vector2 pos)
        {
            foreach (Transform blankTransform in blanksPos)
            {
                Vector2 blankPos = blankTransform.position;

                if (pos == blankPos)
                {
                    DestroyAround(blankPos);
                    blanksPos.Remove(blankTransform);
                    return;
                }
            }
        }

        void DestroyAround(Vector2 pos)
        {
            foreach(Transform squarePos in squares)
            {
                CheckSquareAndBlank(pos, squarePos, 0.5f, 0f);
                CheckSquareAndBlank(pos, squarePos, 0f, 0.5f);
                CheckSquareAndBlank(pos, squarePos, -0.5f, 0f);
                CheckSquareAndBlank(pos, squarePos, 0f, -0.5f);
                CheckSquareAndBlank(pos, squarePos, 0.5f, 0.5f);
                CheckSquareAndBlank(pos, squarePos, -0.5f, -0.5f);
                CheckSquareAndBlank(pos, squarePos, 0.5f, -0.5f);
                CheckSquareAndBlank(pos, squarePos, -0.5f, 0.5f);
            }
        }

        void CheckSquareAndBlank(Vector2 thisPos, Transform target, float x, float y)
        {
            if(target != null)
            {
                Vector2 thisPosMoved = new Vector2(thisPos.x + x, thisPos.y + y);
                Vector2 targetPos = new Vector2(target.position.x, target.position.y);

                if (thisPosMoved == targetPos)
                {
                    for (int i = 0; i < blanksPos.Count; i++)
                    {
                        Vector2 blankPosition = blanksPos[i].position;
                        if (targetPos == blankPosition)
                        {
                            blanksPos.Remove(blanksPos[i]);
                            DestroyAround(blankPosition);
                        }
                    }

                    int counter = 0;
                    for(int i = 0; i < clickedSquares.Count; i++)
                    {
                        if (clickedSquares[i] == target)
                        {
                            counter++;
                        }
                    }

                    if(counter == 0)
                    {
                        clickedSquares.Add(target);
                    }

                    Destroy(target.gameObject);
                    //scoreCounter--;
                }
            }
        }

        void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            gameActive = true;
        }

        public void Easy()
        {
            PlayerPrefs.SetInt("Bombs", 10);
            PlayerPrefs.SetFloat("Line", 4);
            Restart();
        }

        public void Normal()
        {
            PlayerPrefs.SetInt("Bombs", 30);
            PlayerPrefs.SetFloat("Line", 6);
            Restart();
        }

        public void Hard()
        {
            PlayerPrefs.SetInt("Bombs", 60);
            PlayerPrefs.SetFloat("Line", 8);
            Restart();
        }

        void LoseScreen()
        {
            endScreenText.text = "You Lose";
            scoreText.text = "";
            endScreen.SetActive(true);
            gameActive = false;
        }

        void WinScreen()
        {
            endScreenText.text = "You won";
            highScoreText.text = SetHighScore();
            scoreText.text = "Score: " + Mathf.RoundToInt(timeCounter);
            endScreen.SetActive(true);
            gameActive = false;
        }

        string SetHighScore()
        {
            if(PlayerPrefs.GetInt("Bombs") == 10)
            {
                if (PlayerPrefs.GetInt("Easy") == 0) { PlayerPrefs.SetInt("Easy", 999); }
                PlayerPrefs.SetInt("Easy", Mathf.Min(PlayerPrefs.GetInt("Easy"), Mathf.RoundToInt(timeCounter)));
                return "Best Score: " + PlayerPrefs.GetInt("Easy");
            }
            else if(PlayerPrefs.GetInt("Bombs") == 30)
            {
                if(PlayerPrefs.GetInt("Normal") == 0) { PlayerPrefs.SetInt("Normal", 999); }
                PlayerPrefs.SetInt("Normal", Mathf.Min(PlayerPrefs.GetInt("Normal"), Mathf.RoundToInt(timeCounter)));
                return "Best Score: " + PlayerPrefs.GetInt("Normal");
            }
            else if (PlayerPrefs.GetInt("Bombs") == 60)
            {
                if (PlayerPrefs.GetInt("Hard") == 0) { PlayerPrefs.SetInt("Hard", 999); }
                PlayerPrefs.SetInt("Hard", Mathf.Min(PlayerPrefs.GetInt("Hard"), Mathf.RoundToInt(timeCounter)));
                return "Best Score: " + PlayerPrefs.GetInt("Hard");
            }

            return null;
        }
    }
}
