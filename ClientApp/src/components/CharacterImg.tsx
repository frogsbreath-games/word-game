import * as React from "react";
import blank from "../assets/BlankCharacter.png";
import researcher from "../assets/Researcher1.png";
import researcher2 from "../assets/Researcher2.png";
import researcher3 from "../assets/Researcher3.png";
import researcher4 from "../assets/Researcher4.png";
import soldier from "../assets/Soldier1.png";
import soldier2 from "../assets/Soldier2.png";
import soldier3 from "../assets/Soldier3.png";
import soldier4 from "../assets/Soldier4.png";
import cultist from "../assets/Cultist1.png";
import sorceress from "../assets/Cultist2.png";

type CharacterImgProps = {
  number?: number;
};

function characterSrc(number?: number) {
  switch (number) {
    case 0:
      return sorceress;
    case 1:
      return cultist;
    case 2:
      return soldier;
    case 3:
      return researcher;
    case 4:
      return soldier2;
    case 5:
      return researcher2;
    case 6:
      return soldier3;
    case 7:
      return researcher3;
    case 8:
      return soldier4;
    case 9:
      return researcher4;
    default:
      return blank;
  }
}

function characterName(number?: number) {
  switch (number) {
    case 0:
      return "Azami D'aathess";
    case 1:
      return "Z'arri Zuibberh";
    case 2:
      return "Dmitry Koshkin";
    case 3:
      return "Father Moore";
    case 4:
      return "Osip Belinsky";
    case 5:
      return "Inspector Bernard";
    case 6:
      return "Tatyana Ulanov";
    case 7:
      return "Professor Womack";
    case 8:
      return "Komandarm Yeltsin";
    case 9:
      return "Dr Eloise Winthrop";
    default:
      return "None";
  }
}

const CharacterImg = ({ number }: CharacterImgProps) => (
  <img
    src={characterSrc(number)}
    style={{ maxHeight: "100%", maxWidth: "100%", zIndex: 100 }}
    alt={characterName(number)}
  />
);

export default CharacterImg;
