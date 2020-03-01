import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import PlayerTile from "./PlayerTile";
import { CSSTransition, TransitionGroup } from "react-transition-group";
import LogoFooter from "./LogoFooter";
import fadeTransition from "../styles/transitions/fade.module.css";
import styles from "./Lobby.module.css";
import QRCode from "qrcode.react";
import CopyToClipboard from "react-copy-to-clipboard";

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
    this.handleCharacterChange = this.handleCharacterChange.bind(this);
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

  public handleCharacterChange(
    player: GameStore.Player,
    number: number
  ) {
    player.characterNumber = number;
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
          this.state.input
        )
        .catch(err => console.error(err));
      this.setState({ input: "" });
    }
  }

  getJoinUrl(): string {
    return "https://" + window.location.host + "/join/" + this.props.game.code;
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
    player.characterNumber = undefined;
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
      this.props.game.localPlayer.role === "organizer"
    ) {
      organizerButtons = (
        <div className="row mx-auto">
          <button
            disabled={!this.props.game.actions.canGenerateBoard}
            className={styles.submit}
            type="button"
            onClick={() => this.props.generateBoard()}
            style={{ margin: "5px" }}
          >
            Generate Board
          </button>
          <button
            className={styles.delete}
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
            <div className={styles.lobbyBody}>
              <div className={styles.main}>
                <div className={styles.lobbyHeader}>
                  <div>
                    <CopyToClipboard
                      text={this.getJoinUrl()}>
                      <h1
                        title="Click To Copy Game URL"
                        style={{ cursor: "pointer" }}
                      >
                        Lobby: {this.props.game.code}
                      </h1>
                    </CopyToClipboard>
                    <QRCode value={this.getJoinUrl()} />
                    {organizerButtons}
                  </div>
                  <div>
                    <h2>{this.props.game.descriptions.status}</h2>
                    <h6>{this.props.game.descriptions.statusDescription}</h6>
                    <p>{this.props.game.descriptions.localPlayerInstruction}</p>
                  </div>
                </div>
                <div className={styles.playerLists}>
                  <div>
                    <h1 className={styles.red}>Red</h1>
                    <hr />
                    <div className={styles.list}>
                      <TransitionGroup>
                        {this.props.game.players
                          .filter(player => player.team === "red")
                          .map(player => (
                            <CSSTransition
                              key={player.name}
                              timeout={500}
                              classNames={fadeTransition}
                              unmountOnExit
                            >
                              <PlayerTile
                                player={player}
                                localPlayer={this.props.game.localPlayer}
                                gameActions={this.props.game.actions}
                                availableCharacters={this.props.game.availableCharacters}
                                code={this.props.game.code}
                                key={player.number}
                                swapTeams={this.swapTeams}
                                leaveGame={this.quitGame}
                                deleteBot={this.deleteBot}
                                changeCharacter={this.handleCharacterChange}
                              />
                            </CSSTransition>
                          ))}
                      </TransitionGroup>
                      {this.props.game.actions.canAddBot && (this.props.game.players.filter(player => player.team === "red").length < 5) && (
                        <button
                          disabled={!this.props.game.actions.canAddBot}
                          className={styles.button}
                          type="button"
                          onClick={() => this.props.addBot("red")}
                          style={{ margin: "5px", float: "right" }}
                        >
                          Add Red Bot
                          </button>
                      )}
                    </div>
                  </div>
                  <div>
                    <h1 className={styles.blue}>Blue</h1>
                    <hr />
                    <div className={styles.list}>
                      <TransitionGroup>
                        {this.props.game.players
                          .filter(player => player.team === "blue")
                          .map(player => (
                            <CSSTransition
                              key={player.name}
                              timeout={500}
                              classNames={fadeTransition}
                              unmountOnExit
                            >
                              <PlayerTile
                                player={player}
                                localPlayer={this.props.game.localPlayer}
                                gameActions={this.props.game.actions}
                                availableCharacters={this.props.game.availableCharacters}
                                code={this.props.game.code}
                                key={player.number}
                                swapTeams={this.swapTeams}
                                leaveGame={this.quitGame}
                                deleteBot={this.deleteBot}
                                changeCharacter={this.handleCharacterChange}
                              />
                            </CSSTransition>
                          ))}
                      </TransitionGroup>
                      {this.props.game.actions.canAddBot && this.props.game.players.filter(player => player.team === "blue").length < 5 && (
                        <button
                          disabled={!this.props.game.actions.canAddBot}
                          className={styles.button}
                          type="button"
                          onClick={() => this.props.addBot("blue")}
                          style={{ margin: "5px", float: "right" }}
                        >
                          Add Blue Bot
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
              <div className={styles.chat}>
                <div className={styles.eventWindow}>
                  {this.props.events &&
                    this.props.events.map((event, index) => (
                      <div key={index}>
                        <div style={{ margin: "2px" }}>
                          <span
                            style={{
                              fontSize: "10px",
                              fontFamily: "monospace"
                            }}
                          >
                            {event.timestamp}
                          </span>
                          <br />
                          <span className={styles[event.team]}>
                            {event.player ? event.player : "Team " + event.team}
                          </span>
                          <span>{" " + event.message}</span>
                        </div>
                      </div>
                    ))}
                </div>
                <div className={styles.chatInput}>
                  <input
                    className={styles.input}
                    type="text"
                    placeholder="Message something..."
                    value={this.state.input}
                    onChange={this.handleChange}
                    onKeyPress={this.handleKeyPress}
                  />
                </div>
              </div>
            </div>
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
