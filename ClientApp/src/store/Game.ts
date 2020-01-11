import { Action, Reducer } from "redux";
import { AppThunkAction } from "./";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface GameState {
  isLoading: boolean;
  game: Game;
}

export interface Game {
  code: string;
  status: string;
  canStart: boolean;
  players: Player[];
  wordTiles: WordTile[];
}

export interface Player {
  number: number;
  name: string;
  isOrganizer: boolean;
  isSpyMaster: boolean;
  team: string;
}

export interface WordTile {
  word: string;
  team: string;
  isRevealed: boolean;
}

export interface APIResponse {
  message: string;
  data: object;
  errorArray: object[];
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestNewGameAction {
  type: "REQUEST_NEW_GAME";
}

interface ReceiveNewGameAction {
  type: "RECEIVE_NEW_GAME";
  game: Game;
}

interface RequestCurrentGameAction {
  type: "REQUEST_CURRENT_GAME";
}

interface ReceiveCurrentGameAction {
  type: "RECEIVE_CURRENT_GAME";
  game: Game;
}

interface RequestUpdatePlayerAction {
  type: "REQUEST_UPDATE_PLAYER";
}

interface RecieveUpdatePlayerAction {
  type: "RECEIVE_UPDATE_PLAYER";
  player: Player;
}

interface RequestJoinGameAction {
  type: "REQUEST_JOIN_GAME";
}

interface RecieveJoinedGameAction {
  type: "RECEIVE_JOIN_GAME";
  game: Game;
}

interface RequestStartGameAction {
  type: "REQUEST_START_GAME";
}

interface RecieveStartGameAction {
  type: "RECEIVE_START_GAME";
  game: Game;
}

interface RequestLeaveDeleteAction {
  type: "REQUEST_DELETE_GAME";
}

interface RecieveLeaveDeleteAction {
  type: "RECEIVE_DELETE_GAME";
}

interface RequestAddBotAction {
  type: "REQUEST_BOT_PLAYER";
}

interface RecieveAddBotAction {
  type: "RECEIVE_BOT_PLAYER";
  player: Player;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestNewGameAction
  | ReceiveNewGameAction
  | RequestUpdatePlayerAction
  | RecieveUpdatePlayerAction
  | RequestJoinGameAction
  | RecieveJoinedGameAction
  | RequestLeaveDeleteAction
  | RecieveLeaveDeleteAction
  | RequestCurrentGameAction
  | ReceiveCurrentGameAction
  | RequestStartGameAction
  | RecieveStartGameAction
  | RequestAddBotAction
  | RecieveAddBotAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
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
        type: "REQUEST_NEW_GAME"
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
        type: "REQUEST_CURRENT_GAME"
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
          dispatch({
            type: "RECEIVE_UPDATE_PLAYER",
            player: data.data as Player
          });
        });

      dispatch({
        type: "REQUEST_UPDATE_PLAYER"
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
        type: "REQUEST_JOIN_GAME"
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
          dispatch({
            type: "RECEIVE_START_GAME",
            game: data.data as Game
          });
        });

      dispatch({
        type: "REQUEST_START_GAME"
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
        type: "REQUEST_DELETE_GAME"
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
          dispatch({
            type: "RECEIVE_BOT_PLAYER",
            player: data.data as Player
          });
        });

      dispatch({
        type: "REQUEST_BOT_PLAYER"
      });
    }
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: GameState = {
  isLoading: false,
  game: {} as Game
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
    case "REQUEST_CURRENT_GAME":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_CURRENT_GAME":
      return {
        isLoading: false,
        game: action.game
      };
    case "REQUEST_NEW_GAME":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_NEW_GAME":
      return {
        isLoading: false,
        game: action.game
      };
    case "REQUEST_JOIN_GAME":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_JOIN_GAME":
      return {
        isLoading: false,
        game: action.game
      };
    case "REQUEST_START_GAME":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_START_GAME":
      return {
        isLoading: false,
        game: action.game
      };
    case "REQUEST_DELETE_GAME":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_DELETE_GAME":
      return {
        isLoading: false,
        game: {} as Game
      };
    case "REQUEST_UPDATE_PLAYER":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_UPDATE_PLAYER":
      var updatedPlayers = state.game.players.filter(
        player => player.number !== action.player.number
      );
      updatedPlayers.push(action.player);
      return {
        isLoading: false,
        game: { ...state.game, players: updatedPlayers }
      };
    case "REQUEST_BOT_PLAYER":
      return {
        isLoading: true,
        game: state.game
      };
    case "RECEIVE_BOT_PLAYER":
      var players = state.game.players.slice();
      players.splice(action.player.number, 0, action.player);
      return {
        isLoading: false,
        game: { ...state.game, players: players }
      };
    default:
      return state;
  }
};
