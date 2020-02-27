import * as React from "react";
import blank from "../assets/BlankCharacter.png";
import researcher from "../assets/researcher1.png";
import researcher2 from "../assets/Researcher2.png";
import researcher3 from "../assets/Researcher3.png";
import researcher4 from "../assets/researcher4.png";
import soldier from "../assets/Soldier1.png";
import soldier2 from "../assets/Soldier2.png";
import soldier3 from "../assets/Soldier3.png";
import soldier4 from "../assets/Soldier4.png";
import cultist from "../assets/Cultist.png";
import sorceress from "../assets/Cultist2.png";

type CharacterImgProps = {
  number?: number
};

function characterSrc(number?: number) {
  switch (number) {
    case 0: return sorceress;
    case 1: return cultist;
    case 2: return soldier;
    case 3: return researcher;
    case 4: return soldier2;
    case 5: return researcher2;
    case 6: return soldier3;
    case 7: return researcher3;
    case 8: return soldier4;
    case 9: return researcher4;
    default: return blank;
  }
}

function characterName(number?: number) {
  switch (number) {
    case 0: return "Cultist";
    case 1: return "Cultist";
    case 2: return "Researcher 1";
    case 3: return "Researcher 1";
    case 4: return "Researcher 2";
    case 5: return "Researcher 2";
    case 6: return "Researcher 3";
    case 7: return "Researcher 3";
    case 8: return "Researcher 4";
    case 9: return "Researcher 4";
    default: return "None";
  }
}

const CharacterImg = ({
  number
}: CharacterImgProps) => (
    <img
      src={characterSrc(number)}
      style={{ maxHeight: "100%", maxWidth: "100%", zIndex: 100 }}
      alt="{characterName(number)}"
    />
  );

export default CharacterImg;