import { Action, Reducer } from "redux";
import { AppThunkAction } from "./";
import * as signalr from "@aspnet/signalr";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export type Team = "red" | "blue";

export function GetOpponent(team: Team): Team {
  switch (team) {
    case "red":
      return "blue";
    case "blue":
      return "red";
  }
}

export function GetTilesRemaining(game: Game, team: Team): number {
  switch (team) {
    case "red":
      return game.redTilesRemaining;
    case "blue":
      return game.blueTilesRemaining;
  }
}

export type TileColor = Team | "black" | "neutral" | "unknown";

export interface GameState {
  isLoading: boolean;
  game: Game;
  connection?: signalR.HubConnection;
  events: GameEvent[];
}

export interface GameEvent {
  player?: string;
  team: Team;
  timestamp: string;
  type: string;
  message: string;
  data?: object;
}

export interface Game {
  code: string;
  status: string;
  localPlayer: Player;
  players: Player[];
  wordTiles: WordTile[];
  currentTurn?: Turn;
  blueTilesRemaining: number;
  redTilesRemaining: number;
  actions: GameActions;
  descriptions: Descriptions;
}

export interface GameActions {
  canStart: boolean;
  canRestart: boolean;
  canDelete: boolean;
  canAddBot: boolean;
  canDeleteBot: boolean;
  canGiveHint: boolean;
  canApproveHint: boolean;
  canVote: boolean;
}

export interface Descriptions {
  status: string;
  statusDescription: string;
  localPlayerInstruction: string;
}

export interface Turn {
  turnNumber: number;
  team: Team;
  status: string;
  hintWord?: string;
  wordCount?: number;
  guessesRemaining?: number;
  endTurnVotes: PlayerVote[];
}

export interface Player {
  number: number;
  name: string;
  type: PlayerType;
  role: UserRole;
  team: Team;
}

export type PlayerType = "cultist" | "researcher";

export type UserRole = "organizer" | "player" | "bot";

export interface WordTile {
  word: string;
  team: TileColor;
  isRevealed: boolean;
  votes: PlayerVote[];
}

export interface PlayerVote {
  number: number;
  name: string;
  team: Team;
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

interface ReceiveGameEvent {
  type: "RECEIVE_GAME_EVENT";
  event: GameEvent;
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
  | ReceiveUpdateGameAction
  | ReceiveGameEvent;

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

      connection.on("GameUpdated", data => {
        console.log("Game Updated!");
        console.log(data);
        dispatch({
          type: "RECEIVE_UPDATE_GAME",
          game: data as Game
        });
      });

      connection.on("GameEvent", data => {
        console.log("Game Event Received!");
        console.log(data);
        dispatch({
          type: "RECEIVE_GAME_EVENT",
          event: data as GameEvent
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
          debugger;
          console.log(data);
          //This message property is being capitalized
          if (
            data.message !== null &&
            data.message.includes("Cannot find game with code:")
          ) {
            fetch(`api/games/forceSignOut`, {
              method: "POST"
            })
              .then(response => response.json() as Promise<APIResponse>)
              .then(data => {
                console.log(data);
                dispatch({
                  type: "RECEIVE_CURRENT_GAME",
                  game: {} as Game
                });
              });
          } else {
            dispatch({
              type: "RECEIVE_CURRENT_GAME",
              game: data.data as Game
            });
          }
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
  voteEndTurn: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${appState.game.game.code}/voteEndTurn`, {
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
  giveHint: (hint: Hint): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/current/giveHint`, {
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
      fetch(`api/games/current/approveHint`, {
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
  refuseHint: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/current/refuseHint`, {
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
        })
        .catch(error => {
          alert(error);
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
  backToLobby: (code: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    if (appState && appState.game) {
      fetch(`api/games/${code}/backToLobby`, {
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
  game: {} as Game,
  events: [] as GameEvent[]
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
        game: state.game,
        connection: state.connection,
        events: state.events
      };
    case "CREATE_HUB_CONNECTION":
      return {
        isLoading: false,
        game: state.game,
        connection: action.connection,
        events: state.events
      };
    case "RECEIVE_CURRENT_GAME":
      return {
        isLoading: false,
        game: action.game,
        connection: state.connection,
        events: state.events
      };
    case "RECEIVE_NEW_GAME":
      return {
        isLoading: false,
        game: action.game,
        connection: state.connection,
        events: state.events
      };
    case "RECEIVE_JOIN_GAME":
      return {
        isLoading: false,
        game: action.game,
        connection: state.connection,
        events: state.events
      };
    case "RECEIVE_DELETE_GAME":
      return {
        isLoading: false,
        game: {} as Game,
        events: [] as GameEvent[]
      };
    case "RECEIVE_UPDATE_GAME":
      return {
        isLoading: false,
        game: action.game,
        connection: state.connection,
        events: state.events
      };
    case "RECEIVE_GAME_EVENT":
      return {
        isLoading: state.isLoading,
        game: state.game,
        connection: state.connection,
        events: [action.event, ...state.events]
      };
    default:
      return state;
  }
};
