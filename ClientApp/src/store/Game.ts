import { Action, Reducer } from "redux";
import { AppThunkAction } from "./";
import * as signalr from "@aspnet/signalr";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface GameState {
  isLoading: boolean;
  localPlayer: Player;
  game: Game;
  connection?: signalR.HubConnection;
  messages: Message[];
}

export interface Game {
  code: string;
  status: string;
  players: Player[];
  wordTiles: WordTile[];
  currentTurn?: Turn;
  blueTilesRemaining: number;
  redTilesRemaining: number;
  actions: GameActions;
}

export interface GameActions {
  canStart: boolean;
  canDelete: boolean;
  canAddBot: boolean;
  canDeleteBot: boolean;
  canGiveHint: boolean;
  canApproveHint: boolean;
  canVote: boolean;
}

export interface Turn {
  turnNumber: number;
  team: string;
  status: string;
  hintWord?: string;
  wordCount?: number;
  guessesRemaining?: number;
}

export interface Player {
  number: number;
  name: string;
  isOrganizer: boolean;
  isSpyMaster: boolean;
  isBot: boolean;
  team: string;
}

export interface WordTile {
  word: string;
  team: string;
  isRevealed: boolean;
  votes: PlayerVote[];
}

export interface PlayerVote {
  number: number;
  name: string;
  team: string;
}

export interface Hint {
  hintWord: string;
  wordCount: number;
}

export interface Vote {
  word: string;
}

export interface APIResponse {
  message: string;
  data: object;
  errorArray: object[];
}

export interface Message {
  name: string;
  message: Message;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
interface RequestServerAction {
  type: "REQUEST_SERVER_ACTION";
}

interface CreateHubConnectionAction {
  type: "CREATE_HUB_CONNECTION";
  connection: signalR.HubConnection;
}

interface ReceiveNewGameAction {
  type: "RECEIVE_NEW_GAME";
  game: Game;
}

interface ReceiveCurrentGameAction {
  type: "RECEIVE_CURRENT_GAME";
  game: Game;
}

interface ReceiveCurrentPlayerAction {
  type: "RECEIVE_CURRENT_PLAYER";
  localPlayer: Player;
}

interface ReceiveJoinedGameAction {
  type: "RECEIVE_JOIN_GAME";
  game: Game;
}

interface ReceiveLeaveDeleteAction {
  type: "RECEIVE_DELETE_GAME";
}

interface ReceiveUpdateGameAction {
  type: "RECEIVE_UPDATE_GAME";
  game: Game;
}
interface ReceiveMessage {
  type: "RECEIVE_MESSAGE";
  message: Message;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestServerAction
  | CreateHubConnectionAction
  | ReceiveNewGameAction
  | ReceiveJoinedGameAction
  | ReceiveLeaveDeleteAction
  | ReceiveCurrentGameAction
  | ReceiveCurrentPlayerAction
  | ReceiveUpdateGameAction
  | ReceiveMessage;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  createConnection: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    // Only load data if it's something we don't already have (and are not already loading)
    if (appState && appState.game && !appState.game.connection) {
      const connection = new signalr.HubConnectionBuilder()
        .withUrl(`hubs/game`)
        .build();

      connection.on("MessageSent", data => {
        console.log(data.message);
        dispatch({
          type: "RECEIVE_MESSAGE",
          message: data as Message
        });
      });

      connection.on("GameUpdated", data => {
        console.log("Game Updated!");
        console.log(data);
        dispatch({
          type: "RECEIVE_UPDATE_GAME",
          game: data as Game
        });
      });

      connection.on("GameDeleted", data => {
        console.log("Game Deleted!");
        console.log(data);
        dispatch({
          type: "RECEIVE_DELETE_GAME"
        });
      });

      connection
        .start()
        .then(() =>
          dispatch({
            type: "CREATE_HUB_CONNECTION",
            connection: connection
          })
        )
        .then(() => console.log("Connection started!"))
        .catch(err => console.log("Error while establishing connection :("));
    }
  },
  requestNewGame: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games`, { method: "POST" })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
          dispatch({
            type: "RECEIVE_NEW_GAME",
            game: data.data as Game
          });
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  requestCurrentGame: (): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/current`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json"
        }
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
          dispatch({
            type: "RECEIVE_CURRENT_GAME",
            game: data.data as Game
          });
        })
        .catch(error => {
          //added this because if user isn't authenticated get 404 response
          dispatch({
            type: "RECEIVE_CURRENT_GAME",
            game: {} as Game
          });
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  requestCurrentPlayer: (): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/current/players/self`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json"
        }
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
          dispatch({
            type: "RECEIVE_CURRENT_PLAYER",
            localPlayer: data.data as Player
          });
          //if we find a current player we need to get the game they are in
          fetch(`api/games/current`, { method: "GET" })
            .then(response => response.json() as Promise<APIResponse>)
            .then(data => {
              console.log(data);
              dispatch({
                type: "RECEIVE_CURRENT_GAME",
                game: data.data as Game
              });
              dispatch({ type: "REQUEST_SERVER_ACTION" });
            });
        })
        .catch(error => {
          //added this because if user isn't authenticated get 404 response
          dispatch({
            type: "RECEIVE_CURRENT_PLAYER",
            localPlayer: {} as Player
          });
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  updatePlayer: (player: Player): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${appState.game.game.code}/players/${player.number}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(player)
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  voteWord: (guess: Vote): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${appState.game.game.code}/voteWord`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(guess)
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  giveHint: (hint: Hint): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${appState.game.game.code}/giveHint`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(hint)
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  approveHint: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${appState.game.game.code}/approveHint`, {
        method: "POST"
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  joinGame: (code: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${code}/join`, {
        method: "POST"
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
          dispatch({
            type: "RECEIVE_JOIN_GAME",
            game: data.data as Game
          });
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  startGame: (code: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${code}/start`, {
        method: "POST"
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  deleteGame: (code: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${code}`, {
        method: "DELETE"
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
          dispatch({
            type: "RECEIVE_DELETE_GAME"
          });
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  quitGame: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/current/quit`, {
        method: "POST"
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          //Should have same action behavior as delete game
          console.log(data);
          dispatch({
            type: "RECEIVE_DELETE_GAME"
          });
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  addBot: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/current/players`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        }
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  },
  deleteBot: (playerNumber: number): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${appState.game.game.code}/players/${playerNumber}`, {
        method: "DELETE"
      })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          console.log(data);
        });

      dispatch({
        type: "REQUEST_SERVER_ACTION"
      });
    }
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: GameState = {
  isLoading: false,
  localPlayer: {} as Player,
  game: {} as Game,
  messages: [] as Message[]
};

export const reducer: Reducer<GameState> = (
  state: GameState | undefined,
  incomingAction: Action
): GameState => {
  if (state === undefined) {
    return unloadedState;
  }
  const action = incomingAction as KnownAction;
  switch (action.type) {
    case "REQUEST_SERVER_ACTION":
      return {
        isLoading: true,
        localPlayer: state.localPlayer,
        game: state.game,
        connection: state.connection,
        messages: state.messages
      };
    case "CREATE_HUB_CONNECTION":
      return {
        isLoading: false,
        localPlayer: state.localPlayer,
        game: state.game,
        connection: action.connection,
        messages: state.messages
      };
    case "RECEIVE_MESSAGE": {
      return {
        isLoading: false,
        localPlayer: state.localPlayer,
        game: state.game,
        connection: state.connection,
        messages: [...state.messages, action.message]
      };
    }
    case "RECEIVE_CURRENT_GAME":
      return {
        isLoading: false,
        localPlayer: state.localPlayer,
        game: action.game,
        connection: state.connection,
        messages: state.messages
      };
    case "RECEIVE_CURRENT_PLAYER":
      return {
        isLoading: false,
        localPlayer: action.localPlayer,
        game: state.game,
        connection: state.connection,
        messages: state.messages
      };
    case "RECEIVE_NEW_GAME":
      return {
        isLoading: false,
        localPlayer: action.game.players[0],
        game: action.game,
        connection: state.connection,
        messages: state.messages
      };
    case "RECEIVE_JOIN_GAME":
      return {
        isLoading: false,
        localPlayer: action.game.players[action.game.players.length - 1],
        game: action.game,
        connection: state.connection,
        messages: state.messages
      };
    case "RECEIVE_DELETE_GAME":
      return {
        isLoading: false,
        localPlayer: {} as Player,
        game: {} as Game,
        messages: state.messages
      };
    case "RECEIVE_UPDATE_GAME":
      return {
        isLoading: false,
        localPlayer: state.localPlayer,
        game: action.game,
        connection: state.connection,
        messages: state.messages
      };
    default:
      return state;
  }
};
