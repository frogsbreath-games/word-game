import { Action, Reducer } from "redux";
import { AppThunkAction } from "./";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface GameState {
  isLoading: boolean;
  game: object;
}

export interface Game {
  code: string;
  players: Player[];
}

export interface Player {
  name: string;
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

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestNewGameAction | ReceiveNewGameAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  requestNewGame: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();
    debugger;
    if (appState && appState.game) {
      fetch(`api/games`, { method: "POST" })
        .then(response => response.json() as Promise<APIResponse>)
        .then(data => {
          dispatch({
            type: "RECEIVE_NEW_GAME",
            game: data.data as Game
          });
        });

      dispatch({
        type: "REQUEST_NEW_GAME"
      });
    }
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: GameState = {
  isLoading: false,
  game: {}
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
    default:
      return state;
  }
};
