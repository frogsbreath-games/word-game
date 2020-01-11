import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { value: string };

class GameHome extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { value: "" };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  public componentDidMount() {
    this.ensureDataFetched();
    //When game is retrieved redirect to lobby component
  }

  private ensureDataFetched() {
    this.props.requestCurrentGame();
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ value: event.target.value });
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public render() {
    if (this.props.isLoading === false && this.props.game.status) {
      switch (this.props.game.status) {
        case "inProgress":
          return <Redirect to="/game" />;
        case "lobby":
          return <Redirect to="/lobby" />;
        default:
          break;
      }
    }
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
                value={this.state.value}
                onChange={this.handleChange}
                className="form-control"
                placeholder="XXX69..."
              />
              <div className="input-group-append">
                <button
                  className="btn btn-primary"
                  type="button"
                  onClick={() => this.props.joinGame(this.state.value)}
                >
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
