import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import styles from "./GameHome.module.css";
import { ReactComponent as Cthulhu } from "../assets/Cthulhu.svg";
import { Container } from "reactstrap";
import LogoFooter from "./LogoFooter";
import { RouteComponentProps } from "react-router-dom";

// At runtime, Redux will merge together...
type GameProps = RouteComponentProps<{ gameCode?: string }> &
  GameStore.GameState &
  typeof GameStore.actionCreators;

type State = {
  gameCode: string;
  showError: boolean;
};

class GameHome extends React.Component<GameProps, State> {
  constructor(props: GameProps) {
    super(props);

    this.state = {
      gameCode: "",
      showError: false,
    };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  public componentDidMount() {
    this.ensureDataFetched();

    const gameCode = this.props.match.params.gameCode;

    if (gameCode) {
      this.setState({ gameCode, showError: true });
      this.props.joinGame(gameCode);
    }
    //When game is retrieved redirect to lobby component
  }

  private ensureDataFetched() {
    this.props.requestCurrentGame();
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ gameCode: event.target.value, showError: false });
  }

  handleSubmit(event: React.MouseEvent) {
    if (this.state.gameCode !== undefined && this.state.gameCode !== "") {
      this.props.joinGame(this.state.gameCode);
      this.setState({ ...this.state, showError: true });
    }
    event.preventDefault();
  }

  public render() {
    if (
      this.props.isLoading === false &&
      this.props.game &&
      this.props.game.status
    ) {
      switch (this.props.game.status) {
        case "inProgress":
        case "boardReview":
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
        <div style={{ position: "relative", minHeight: "100vh" }}>
          <div style={{ paddingBottom: "16rem" }}>
            <Container>
              <div style={{ textAlign: "center" }} className={styles.content}>
                <Cthulhu style={{ width: "400px" }} />
                <h1>Have a code?</h1>
                <div className={styles.joinGame}>
                  <input
                    type="text"
                    value={this.state.gameCode}
                    onChange={this.handleChange}
                    className={
                      this.state.showError && this.props.errorMessage
                        ? styles.inputError
                        : styles.input
                    }
                    placeholder="Enter Existing Code Here..."
                  />
                  <div className="input-group-append">
                    <button
                      className={styles.submit}
                      type="button"
                      onClick={this.handleSubmit}
                    >
                      Join Game
                    </button>
                  </div>
                </div>
                {this.props.errorMessage && this.state.showError && (
                  <p className={styles.errorMessage}>
                    {this.props.errorMessage}
                  </p>
                )}
              </div>
              <div style={{ textAlign: "center" }}>
                <h1>Don't have a code?</h1>
                <button
                  type="button"
                  className={styles.submit}
                  style={{ width: 300 }}
                  onClick={() => {
                    this.props.requestNewGame();
                  }}
                >
                  {buttonText}
                </button>
              </div>
            </Container>
          </div>
          <LogoFooter />
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(GameHome as any);
