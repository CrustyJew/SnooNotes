<template>
    <div id="sn-sub-reddit-settings">
        <div class="sn-opts-header">
            <h1 class="sn-sub-opts-header">/r/{{initialSettings.SubName}}</h1>
            <button type="button" class="sn-btn-cancel" id="sn-btn-sub-opts-cancel" @click="cancel">Cancel</button>
            <br style="clear:both;">
        </div>
        <div class="sn-container">
            <div id="sn-access-mask">
                <div id="sn-access-mask-desc">Choose who can view and add notes below. Anyone with full permissions can always view and add notes as well as edit this page</div>
                <div id="sn-access-mask-options">
                    <label>
                        <input type="checkbox" value="1" v-model.number="selectedAccess">access</label>
                    <label>
                        <input type="checkbox" value="2" v-model.number="selectedAccess">config</label>
                    <label>
                        <input type="checkbox" value="4" v-model.number="selectedAccess">flair</label>
                    <label>
                        <input type="checkbox" value="8" v-model.number="selectedAccess">mail</label>
                    <label>
                        <input type="checkbox" value="16" v-model.number="selectedAccess">posts</label>
                    <label>
                        <input type="checkbox" value="32" v-model.number="selectedAccess">wiki</label>
                </div>
            </div>
            <div id="sn-sub-settings-faq">
                <h2>How do I enable automatic generation of ban notes?</h2>
                <div class="sn-faq-answer">Check options for both Temp and Perm ban types (checkboxes to the left of the note type name). People may have to refresh their SnooNotes for it to start</div>
                <h2>What are these Note Type Icons?</h2>
                <div class="sn-faq-answer">They are to help distinguish note types in the preview you get when mousing over a user's note icon. They can be any icon font string from
                    <a href="https://material.io/icons/" target="_blank">here</a>, although brand new ones may not work. To get the string, click the icon you want, click "&lt;&gt; ICON FONT" and copy the string in between the &lt;i&gt; tags</div>
                <h2>Can I set a default note type?</h2>
                <div class="sn-faq-answer">It's on my to-do list. You can re-order the note types by dragging the arrows on the far left though</div>
                <h2>Why isn't TheSentinelBot being recognized for my sub?</h2>
                <div class="sn-faq-answer">It should update roughly every hour. If it still hasn't, meepster23 probably broke something. Go poke him.</div>
            </div>
            <div id="sn-note-types">
                <div id="sn-note-types-desc">Change just about everything about the Note Types belonging to this subreddit below. If no checkbox is chosen for Perm Ban or Temp Ban, then automatic ban notes will not be generated for that type of ban.
                    <br />
                    <br />&nbsp;Temp&nbsp;|&nbsp;Perm
                    <br />&nbsp;&nbsp;&nbsp;Ban&nbsp;&nbsp;|&nbsp;&nbsp;Ban</div>
                <draggable element="ul" :list="newSettings.Settings.NoteTypes" :options="dragOptions">
                    <li :sn-notetype-id="nt.NoteTypeID" :sn-notetype-display-order="nt.DisplayOrder" v-for="nt in newSettings.Settings.NoteTypes" :key="nt.NoteTypeID">
                        <div class="sn-drag-handle">
                            <a class="sn-sort"></a>
                        </div>
                        <input type="checkbox" :value="nt.NoteTypeID" v-on:change="selectTempBan(nt.NoteTypeID, $event)" v-model.number="tempBanID">
                        <input type="checkbox" :value="nt.NoteTypeID" v-on:change="selectPermBan(nt.NoteTypeID, $event)" v-model.number="permBanID">
                        <input type="text" maxlength="20" v-model="nt.DisplayName"> &nbsp;Color:&nbsp;
                        <input type="color" v-model="nt.ColorCode">
                        <input type="text" maxlength="50" placeholder="Icon. Default ='comment'" v-model="nt.IconString">
                        <label>
                            <input type="checkbox" v-model="nt.Bold">Bold</label>
                        <label>
                            <input type="checkbox" v-model="nt.Italic">Italic</label>
                        &nbsp;
                        <span class="sn-preview" :style="getStyle(nt)">
                            <i class="material-icons">{{nt.IconString && nt.IconString.length > 0 ? nt.IconString : 'comment'}}</i>{{nt.DisplayName}}</span>
                        <a class="sn-remove" @click="removeNoteType(nt)">x</a>
                    </li>
                </draggable>
                <div style="text-align:center;" @click="addNoteType">
                    <a class="sn-add">+</a>
                </div>
            </div>
        </div>
        <button type="button" class="sn-btn-submit" id="sn-btn-subopts-save" @click="save" :disabled="saving">{{saving ? "Saving..." : "Save"}}</button>
    </div>
</template>
<script>
import draggable from 'vuedraggable'
import axios from 'axios';
import Toasted from 'vue-toasted';
export default {
    name: "SNSubOptions",
    props: ["sub", "cancel", "finish"],
    components: {
        draggable,
        Toasted
    },
    data() {
        return {
            selectedAccess: [],
            permBanID: [],
            tempBanID: [],
            initialSettings: this.sub,
            newSettings: {},
            saving: false
        }
    },
    methods: {
        selectPermBan(id, e) {
            if (e.target.checked) {
                this.permBanID = [];
                this.permBanID.push(id);
            }
        },
        selectTempBan(id, e) {
            if (e.target.checked) {
                this.tempBanID = [];
                this.tempBanID.push(id);
            }
        },
        getStyle(nt) {
            return {
                color: nt.ColorCode,
                fontWeight: nt.Bold ? 'bold' : 'normal',
                fontStyle: nt.Italic ? 'italic' : 'normal'
            }
        },
        addNoteType() {
            this.newSettings.Settings.NoteTypes.push({
                DisplayName: '',
                ColorCode: '#000000',
                Bold: false,
                Italic: false,
                SubName: this.initialSettings.SubName,
                IconString: 'comment'
            })
        },
        removeNoteType(nt) {
            this.newSettings.Settings.NoteTypes.splice(this.newSettings.Settings.NoteTypes.indexOf(nt), 1);
        },
        save() {
            this.saving = true;
            let newAccessMask = 64;
            for (let i = 0; i < this.selectedAccess.length; i++) {
                newAccessMask += this.selectedAccess[i];
            }
            let subSets = Promise.resolve();
            if (newAccessMask != this.initialSettings.Settings.AccessMask
                || this.initialSettings.Settings.PermBanID != this.permBanID[0]
                || this.initialSettings.Settings.TempBanID != this.tempBanID[0]) {

                //access mask, or auto ban note id changed
                subSets = axios.put('subreddit/' + this.initialSettings.SubName, {
                    Settings: {
                        AccessMask: newAccessMask,
                        TempBanID: this.tempBanID[0],
                        PermBanID: this.permBanID[0]
                    }
                });
            }
            var ntAddData = [];
            var ntDelData = [];
            var ntUpdData = [];
            for (let i = 0; i < this.newSettings.Settings.NoteTypes.length; i++) {
                let nt = this.newSettings.Settings.NoteTypes[i];
                nt.ColorCode = nt.ColorCode.replace('#', '');
                nt.DisplayOrder = i;
                if (nt.NoteTypeID && nt.NoteTypeID > -1) {
                    ntUpdData.push(nt);
                }
                else {
                    ntAddData.push(nt);
                }
            }
            let updIds = ntUpdData.map(n => n.NoteTypeID);
            for (let i = 0; i < this.initialSettings.Settings.NoteTypes.length; i++) {
                let nt = this.initialSettings.Settings.NoteTypes[i];
                if (updIds.indexOf(nt.NoteTypeID) == -1 && !nt.Disabled) {
                    ntDelData.push(nt);
                }
            }
            let ntAdd = !(ntAddData.length) ? Promise.resolve() : axios.post('NoteType', ntAddData);
            let ntDel = !(ntDelData.length) ? Promise.resolve() : axios({ method: 'delete', url: 'NoteType', data: ntDelData });
            let ntUpd = !(ntUpdData.length) ? Promise.resolve() : axios.put('NoteType', ntUpdData);

            Promise.all([ntAdd, ntDel, ntUpd, subSets]).then(() => {
                this.$toasted.success("Saved Settings!");
                this.saving = false;
                this.finish();
            }, () => {
                this.$toasted.error("Failed to save settings!", { duration: null });
                for (let i = 0; i < this.newSettings.Settings.NoteTypes.length; i++) {
                    let nt = this.newSettings.Settings.NoteTypes[i];
                    nt.ColorCode = '#' + nt.ColorCode;
                }
                this.saving = false;
            })
        }
    },
    computed: {
        dragOptions() {
            return {
                animation: 150,
                ghostClass: 'ghost',
                handle: '.sn-drag-handle'
            }
        }
    },
    created: function () {
        if (this.sub.Settings.AccessMask & 0x01) this.selectedAccess.push(1);
        if (this.sub.Settings.AccessMask & 0x02) this.selectedAccess.push(2);
        if (this.sub.Settings.AccessMask & 0x04) this.selectedAccess.push(4);
        if (this.sub.Settings.AccessMask & 0x08) this.selectedAccess.push(8);
        if (this.sub.Settings.AccessMask & 0x10) this.selectedAccess.push(16);
        if (this.sub.Settings.AccessMask & 0x20) this.selectedAccess.push(32);
        if (this.sub.Settings.PermBanID) this.permBanID.push(this.sub.Settings.PermBanID);
        if (this.sub.Settings.TempBanID) this.tempBanID.push(this.sub.Settings.TempBanID);
        for (var i = 0; i < this.sub.Settings.NoteTypes.length; i++) {
            if (this.sub.Settings.NoteTypes[i].ColorCode.indexOf('#') == -1) {
                this.sub.Settings.NoteTypes[i].ColorCode = '#' + this.sub.Settings.NoteTypes[i].ColorCode;
            }
        }
        this.newSettings = JSON.parse(JSON.stringify(this.sub));
        this.newSettings.Settings.NoteTypes = this.newSettings.Settings.NoteTypes.filter(nt => !nt.Disabled);
    }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";

.ghost {
    opacity: .5;
    background: $light-gray;
}

#sn-options-panel #sn-options-container #sn-sub-reddit-settings {

    .sn-sub-opts-header {
        float: left;
        color: $secondary;
        font-weight: bold;
        margin: 0px;
    }

    .sn-opts-header {
        margin-bottom: 10px;
    }

    #sn-btn-sub-opts-cancel {
        float: right;
    }

    #sn-btn-subopts-save {
        margin: 0 auto;
        margin-top: 20px;
        display: block;
    }

    #sn-access-mask {
        width: 200px;
        display: inline-block;
        border: 1px solid transparent;
        border-radius: 5px;
        background-color: $light-gray;
        padding: 5px;
        margin-right: 20px;
        margin-bottom: 20px;
        vertical-align: top;
    }

    #sn-access-mask-options {
        width: 100px;
        label {
            display: block;
        }
        input[type=checkbox] {
            vertical-align: middle;
            margin: 0px 5px 0px 0px;
        }
    }

    #sn-access-mask-desc {
        font-weight: bold;
        margin-bottom: 5px;
    }
    #sn-sub-settings-faq {
        width: 675px;
        display: inline-block;
    }
    #sn-note-types-desc {
        font-weight: bold;
    }

    #sn-note-types {
        display: inline-block;
        width: 900px;
        padding: 5px;
        border: 1px solid transparent;
        background-color: $light-gray;
        border-radius: 5px;
    }
    li {
        border: 1px solid darkgrey;
        border-radius: 3px;
        padding: 2px;
        background-color: $light-gray;
        margin: 1px;
    }

    input {
        vertical-align: middle;
    }

    input[type=checkbox] {
        vertical-align: middle;
        margin: 0px 5px 0px 5px;
        height: 20px;
    }

    a.sn-sort {
        display: inline-block;
        vertical-align: middle;
        height: 6px;
        /*length of Arrow*/
        width: 4px;
        margin: 0 8px;
        background-color: black;
        position: relative;

        &:before {
            content: "";
            position: absolute;
            left: -4px;
            top: -6px;
            border-color: black transparent;
            border-style: solid;
            border-width: 0px 6px 6px 6px;
            height: 0px;
            width: 0px;
        }

        &:after {
            content: "";
            position: absolute;
            left: -4px;
            top: 100%;
            border-color: black transparent;
            border-style: solid;
            border-width: 6px 6px 0px 6px;
            height: 0px;
            width: 0px;
        }
    }

    i.material-icons {
        font-size: 18px;
        vertical-align: middle;
    }

    .sn-preview {
        display: inline-block;
        min-width: 160px;
    }
    .sn-drag-handle {
        float: left;
        height: 26px;
        line-height: 26px;

        cursor: grab;
        cursor: -webkit-grab;
        &:active {
            cursor: grabbing;
            cursor: -webkit-grabbing;
        }
    }
    .sn-remove {
        vertical-align: middle;
        height: 16px;
        width: 16px;
        font-size: 12px;
        line-height: 14px;
        font-weight: bold;
        font-family: verdana;
        color: white;
        text-align: center;
        background: linear-gradient(to bottom, $accent, darken($accent, 15%));
        position: relative;
        border-radius: 50%;
        border: 1px solid darkgrey;
        cursor: pointer;
        top: 5px;
        float: right;
    }

    .sn-add {
        display: inline-block;
        vertical-align: middle;
        height: 16px;
        width: 16px;
        font-size: 12px;
        line-height: 15px;
        font-weight: bold;
        font-family: verdana;
        color: white;
        text-align: center;
        margin-left: 8px;
        background: linear-gradient(to bottom, #74c429, #4ca20b);
        position: relative;
        border-radius: 50%;
        border: 1px solid darkgrey;
        cursor: pointer;
    }
}
</style>