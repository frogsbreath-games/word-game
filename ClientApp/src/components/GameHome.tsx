import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

class GameHome extends React.PureComponent<GameProps> {
  public render() {
    return (
      <React.Fragment>
        <div>
          <h1>Don't have a code?</h1>
          <button
            type="button"
            className="btn btn-primary"
            onClick={() => {
              this.props.requestNewGame();
            }}
          >
            New Game
          </button>
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(GameHome as any);
