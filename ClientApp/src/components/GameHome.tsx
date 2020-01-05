import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

class GameHome extends React.PureComponent<GameProps> {
  public render() {
    let buttonText = "New Game";
    if (this.props.isLoading) {
      buttonText = "Loading";
    } else {
      buttonText = "New Game";
    }
    return (
      <React.Fragment>
        <div>
          <div style={{ textAlign: "center" }}>
            <h1>Have a code?</h1>
            <div
              className="input-group mx-auto"
              style={{ width: 300, marginBottom: 10 }}
            >
              <input
                type="text"
                className="form-control"
                placeholder="XXX69..."
              />
              <div className="input-group-append">
                <button className="btn btn-primary" type="button">
                  Join Game
                </button>
              </div>
            </div>
          </div>
          <div style={{ textAlign: "center" }}>
            <h1>Don't have a code?</h1>
            <button
              type="button"
              className="btn btn-primary"
              style={{ width: 300 }}
              onClick={() => {
                this.props.requestNewGame();
              }}
            >
              {buttonText}
            </button>
          </div>
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(GameHome as any);
