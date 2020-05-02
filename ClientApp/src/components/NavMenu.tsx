import * as React from "react";
import { Collapse, Navbar, NavbarBrand, NavbarToggler } from "reactstrap";
import { Link } from "react-router-dom";
import "./NavMenu.css";
import { ReactComponent as CthulhuLogo } from "../assets/CthulhuIcon.svg";

export default class NavMenu extends React.PureComponent<
  {},
  { isOpen: boolean }
> {
  public state = {
    isOpen: false
  };

  public render() {
    return (
      <header>
        <Navbar
          className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3"
          light
        >
          <CthulhuLogo style={{ width: "40px", marginRight: "10px" }} />
          <NavbarBrand tag={Link} to="/">
            <h3>Spellbook</h3>
          </NavbarBrand>
          <NavbarToggler onClick={this.toggle} className="mr-2" />
          <Collapse
            className="d-sm-inline-flex flex-sm-row-reverse"
            isOpen={this.state.isOpen}
            navbar
          >
            <ul className="navbar-nav flex-grow"></ul>
          </Collapse>
        </Navbar>
      </header>
    );
  }

  private toggle = () => {
    this.setState({
      isOpen: !this.state.isOpen
    });
  };
}
