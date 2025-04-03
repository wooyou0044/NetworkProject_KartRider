## 인게임씬에 4명이 접속했다는 가정
```mermaid
flowchart LR

subgraph User1["User 1 (방장)"]
User1Kart("**Kart1**<br>PhotonView<br>Transform<br>Rigidbody<br>RankManager")
User1Character("**Character1**<br>PhotonView<br>Transform")
User1ItemManager("**ItemMng1**<br>PhotonView")
User1GameManager("**GameMng1**<br>PhotonView")

end

subgraph User2["User 2"]
User2Kart("**Kart2**<br>PhotonView<br>Transform<br>Rigidbody<br>RankManager")
User2Character("**Character2**<br>PhotonView<br>Transform")
User2ItemManager("**ItemMng2**<br>PhotonView")
User2GameManager("**GameMng2**<br>PhotonView")

end

subgraph User3["User 3"]
User3Kart("**Kart3**<br>PhotonView<br>Transform<br>Rigidbody<br>RankManager")
User3Character("**Character3**<br>PhotonView<br>Transform")
User3ItemManager("**ItemMng**<br>PhotonView")
User3GameManager("**GameMng3**<br>PhotonView")

end

subgraph User4["User 4"]
User4Kart("**Kart4**<br>PhotonView<br>Transform<br>Rigidbody<br>RankManager")
User4Character("**Character4**<br>PhotonView<br>Transform")
User4ItemManager("**ItemMng4**<br>PhotonView")
User4GameManager("**GameMng4**<br>PhotonView")

end

User1 <--> Server(("Pun2 Server"))
User2 <--> Server
User3 <--> Server
User4 <--> Server
```

## 게임 매니저 게임 시작시 실행 과정
```mermaid
flowchart LR

subgraph User1["User 1 (방장)"]

User1GameManager("**GameMng1**<br>PhotonView")

end

subgraph User2["User 2"]

User2GameManager("**GameMng2**<br>PhotonView")

end

subgraph User3["User 3"]

User3GameManager("**GameMng3**<br>PhotonView")

end

subgraph User4["User 4"]

User4GameManager("**GameMng4**<br>PhotonView")

end

User1 -- 준비 다 됐대요 --> Server(("Pun2 Server"))
Server -- 시작해 --> User1
Server -- 시작해 --> User2
Server -- 시작해 --> User3
Server -- 시작해 --> User4
User2GameManager -- 레디됨 --> User1GameManager 
User3GameManager -- 레디됨 --> User1GameManager 
User4GameManager -- 레디됨 --> User1GameManager 
```

## 게임 종료시 실행 과정 (ex : 유저 3번이 골인했다)
```mermaid
flowchart LR

subgraph User1["User 1 (방장)"]

User1GameManager("**GameMng1**<br>PhotonView")

end

subgraph User2["User 2"]

User2GameManager("**GameMng2**<br>PhotonView")

end

subgraph User3["User 3"]

User3GameManager("**GameMng3**<br>PhotonView")

end

subgraph User4["User 4"]

User4GameManager("**GameMng4**<br>PhotonView")

end

User3 -- 나 골인함 --> Server(("Pun2 Server"))
Server -- 3번 골인, 카운트다운 센다? --> User1
Server -- 3번 골인, 카운트다운 센다? --> User2
Server -- 3번 골인, 카운트다운 센다? --> User3
Server -- 3번 골인, 카운트다운 센다? --> User4
User1GameManager -- 나도 들어옴 --> Server
User2GameManager x-- 난 못 들어옴 --x Server 
User4GameManager x-- 난 못 들어옴 --x Server

User1GameManager --> 게임종료
User2GameManager --> 게임종료
User3GameManager --> 게임종료
User4GameManager --> 게임종료

```

## ItemManager 
### 1. 물파리를 날린다.  
- 유저4가 4등, 유저 3이 3등이라고 가정.
```mermaid
flowchart LR

subgraph User1["User 1(방장))"]
User1ItemManager("**ItemMng1**<br>PhotonView")
end

User2

subgraph User3["User 3"]
User3Kart("**Kart3**<br>PhotonView<br>Transform<br>Rigidbody")
User3ItemManager("**ItemMng3**<br>PhotonView")
User3SoundManager
end

subgraph User4["User 4"]
User4ItemManager("**ItemMng1**<br>PhotonView")
User4RankManager("RankManager")
end

User4ItemManager -- 3등이 누구야? --> User4RankManager
User4RankManager -- 유저 3번 이야 --> User4ItemManager
User4 -- 방장한테 3번에<br>물파리 날리라고해 --> Server(("Pun2 Server"))
Server -- 3번한테 쏘랍니다 --> User1ItemManager
User1ItemManager -- 묿파리 받아라<br>소리도 재생해 --> User3ItemManager
User3ItemManager -- 2초후, 쉴드 있음? --> User3Kart
User3ItemManager -- 물파리 사운드 재생 --> User3SoundManager
User3Kart -- 쉴드 없음<br>나 맞았다? --> Server
Server -- 3번 물파리 맞았대 --> User1
Server -- 3번 물파리 맞았대 --> User2
Server -- 3번 물파리 맞았대 --> User4
```

### 2. 바리케이트를 날린다.
* 유저 4가 바리케이트 발사, 유저 1이 1등이라고 가정
```mermaid
flowchart LR

subgraph User1["User 1"]
User1ItemManager("**ItemMng1**<br>PhotonView")
User1RankManager("RankManager")
end

User2
User3

subgraph User4["User 4"]
User4ItemManager("**ItemMng1**<br>PhotonView")
User4RankManager("RankManager")
end

User4ItemManager -- 1등이 누구야? --> User4RankManager
User4RankManager -- 유저 1번 이야 --> User4ItemManager
User4 -- 1번한테 바리케이트 날려라 --> Server(("Pun2 Server"))
Server -- 바리케이트 설치하세요 --> User1ItemManager
User1ItemManager -- 너 어디쯤이니? --> User1RankManager
User1RankManager -- 내 앞에 체크포인트에 바리케이트 깔았다 --> Server
Server -- 바리케이트 깔았대 --> User2
Server -- 바리케이트 깔았대 --> User3
Server -- 바리케이트 깔았대 --> User4
```