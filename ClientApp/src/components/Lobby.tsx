import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import PlayerTile from "./PlayerTile";
import { ReactComponent as MessageIcon } from "../assets/MessageIcon.svg";
import { red, blue } from "../constants/ColorConstants";
import { CSSTransition, TransitionGroup } from "react-transition-group";
import "./Lobby.css";
import { Container } from "reactstrap";
import LogoFooter from "./LogoFooter";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { input: string };
class Lobby extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { input: "" };
    this.sendMessage = this.sendMessage.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.swapTeams = this.swapTeams.bind(this);
    this.deleteBot = this.deleteBot.bind(this);
    this.quitGame = this.quitGame.bind(this);
    this.handleRoleChange = this.handleRoleChange.bind(this);
  }

  public componentDidMount() {
    //game has not been retrieved do not create a connection ever
    if (this.props.game.status) {
      //if we are supposed to be in the lobby and we don't have a connection make one
      if (this.props.game.status === "lobby" && !this.props.connection) {
        this.ensureConnectionExists();
      }
    }
  }

  private ensureConnectionExists() {
    this.props.createConnection();
  }

  public handleRoleChange(player: GameStore.Player) {
    if (player.isSpyMaster) {
      player.isSpyMaster = false;
    } else {
      player.isSpyMaster = true;
    }
    this.props.updatePlayer(player);
  }

  public handleKeyPress(event: React.KeyboardEvent) {
    var keyCode = event.keyCode || event.which;
    if (keyCode === 13) {
      this.sendMessage();
    }
  }

  public sendMessage() {
    if (this.props.connection) {
      this.props.connection
        .invoke(
          "SendMessage",
          this.state.input,
          this.props.game.localPlayer.team
        )
        .catch(err => console.error(err));
      this.setState({ input: "" });
    }
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ input: event.target.value });
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public swapTeams(player: GameStore.Player) {
    if (player.team === "blue") {
      player.team = "red";
    } else {
      player.team = "blue";
    }
    this.props.updatePlayer(player);
  }

  public deleteGame(code: string) {
    this.props.deleteGame(code);
  }

  public quitGame() {
    this.props.quitGame();
  }

  public deleteBot(playerNumber: number) {
    this.props.deleteBot(playerNumber);
  }

  public render() {
    if (!this.props.game.status || this.props.game.status !== "lobby") {
      return <Redirect to="/game-home" />;
    }

    let organizerButtons;
    if (
      this.props.game.localPlayer &&
      this.props.game.localPlayer.isOrganizer
    ) {
      organizerButtons = (
        <div className="row mx-auto">
          <button
            disabled={!this.props.game.actions.canStart}
            className="btn btn-primary"
            type="button"
            onClick={() => this.props.startGame(this.props.game.code)}
            style={{ margin: "5px" }}
          >
            Start Game
          </button>
          <button
            disabled={!this.props.game.actions.canAddBot}
            className="btn btn-secondary"
            type="button"
            onClick={() => this.props.addBot()}
            style={{ margin: "5px" }}
          >
            Add Bot
          </button>
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => this.props.requestCurrentGame()}
            style={{ margin: "5px" }}
          >
            Refresh
          </button>
          <button
            className="btn btn-danger"
            type="button"
            onClick={() => this.props.deleteGame(this.props.game.code)}
            style={{ margin: "5px" }}
          >
            Delete Game
          </button>
        </div>
      );
    }

    return (
      <React.Fragment>
        <div style={{ position: "relative", minHeight: "100vh" }}>
          <div style={{ paddingBottom: "16rem" }}>
            <Container>
              <div style={{ display: "grid", gridTemplateColumns: "50% 50%" }}>
                <div>
                  <h1>Lobby: {this.props.game.code}</h1>
                  {organizerButtons}
                </div>
                <div>
                  <h3>{this.props.game.descriptions.status}</h3>
                  <h6>{this.props.game.descriptions.statusDescription}</h6>
                  <p>{this.props.game.descriptions.localPlayerInstruction}</p>
                </div>
              </div>
              <div className="row">
                <div className="col-sm-6">
                  <h1 style={{ color: red }}>Red</h1>
                  <hr />
                  <TransitionGroup>
                    {this.props.game.players
                      .filter(player => player.team === "red")
                      .map(player => (
                        <CSSTransition
                          key={player.name}
                          timeout={500}
                          classNames="item"
                        >
                          <PlayerTile
                            player={player}
                            localPlayer={this.props.game.localPlayer}
                            gameActions={this.props.game.actions}
                            code={this.props.game.code}
                            key={player.number}
                            swapTeams={this.swapTeams}
                            leaveGame={this.quitGame}
                            deleteBot={this.deleteBot}
                            changeRole={this.handleRoleChange}
                          />
                        </CSSTransition>
                      ))}
                  </TransitionGroup>
                </div>
                <div className="col-sm-6">
                  <h1 style={{ color: blue }}>Blue</h1>
                  <hr />
                  <TransitionGroup>
                    {this.props.game.players
                      .filter(player => player.team === "blue")
                      .map(player => (
                        <CSSTransition
                          key={player.name}
                          timeout={500}
                          classNames="item"
                        >
                          <PlayerTile
                            player={player}
                            localPlayer={this.props.game.localPlayer}
                            gameActions={this.props.game.actions}
                            code={this.props.game.code}
                            key={player.number}
                            swapTeams={this.swapTeams}
                            leaveGame={this.quitGame}
                            deleteBot={this.deleteBot}
                            changeRole={this.handleRoleChange}
                          />
                        </CSSTransition>
                      ))}
                  </TransitionGroup>
                </div>
              </div>
              <div className="row" style={{ marginTop: "20px" }}>
                <div className="input-group mb-3">
                  <div className="input-group-prepend">
                    <span
                      className="input-group-text"
                      id="inputGroup-sizing-default"
                    >
                      <MessageIcon
                        style={{
                          opacity: 0.5,
                          height: "24px",
                          fill: "black"
                        }}
                      />
                    </span>
                  </div>
                  <input
                    className="form-control"
                    type="text"
                    placeholder="Message something..."
                    value={this.state.input}
                    onChange={this.handleChange}
                    onKeyPress={this.handleKeyPress}
                  />
                </div>
              </div>
              {this.props.events &&
                this.props.events.map((event, index) => (
                  <div key={index}>
                    <div style={{ margin: "2px" }}>
                      {event.player + ": " + event.message}
                    </div>
                  </div>
                ))}
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
)(Lobby as any);
