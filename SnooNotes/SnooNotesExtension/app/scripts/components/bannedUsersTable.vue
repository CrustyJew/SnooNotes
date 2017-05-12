<template>
    <div class="sn-search-table md-card md-table-card" v-if="adminSubs && adminSubs.length > 0">
    
        <h1 class="md-title">Bot Banned Users</h1>
    
        <div>
            <button @click="toggleAll" class="sn-toggle-all">Select/Unselect all</button>
            <label v-for="sub in adminSubs" class="sn-search-sub-label" :class="{checked: searchSubs.indexOf(sub.SubName) > -1}">
                <input type="checkbox" :value="sub.SubName" v-model="searchSubs" /> {{sub.SubName}}
            </label>
        </div>
    
        <md-input-container>
            <md-icon>search</md-icon>
            <label>Search</label>
            <md-textarea v-model="searchTerm"></md-textarea>
        </md-input-container>
        <!--<paginator :rows="rowsPerPage" :page="currentPage" :total="totalRows"></paginator>-->
        <div class="sn-table-area">
    
            <md-table md-sort="date" md-sort-type="desc" @sort="sortTable">
                <md-table-header>
                    <md-table-row>
                        <md-table-head md-sort-by="date">Date</md-table-head>
                        <md-table-head md-sort-by="username">User</md-table-head>
                        <md-table-head md-sort-by="subname">Subreddit</md-table-head>
                        <md-table-head md-sort-by="bannedby">Banned By</md-table-head>
                        <md-table-head md-sort-by="reason">Reason</md-table-head>
                        <md-table-head>Banned For</md-table-head>
                        <md-table-head>Additional Info</md-table-head>
                        <md-table-head></md-table-head>
                    </md-table-row>
                </md-table-header>
    
                <md-table-body v-if="!loadingResults">
                    <md-table-row v-for="(ban, banIndex) in searchResults" :md-item="ban" :key="banIndex">
                        <md-table-cell style="white-space:pre;text-align:center;">{{new Date(ban.BanDate).toLocaleString().replace(', ', '\n')}}</md-table-cell>
                        <md-table-cell>{{ban.UserName}}</md-table-cell>
                        <md-table-cell>{{ban.SubName}}</md-table-cell>
                        <md-table-cell>{{ban.BannedBy}}</md-table-cell>
                        <md-table-cell>{{ban.BanReason}}</md-table-cell>
                        <md-table-cell>{{ban.ThingURL}}</md-table-cell>
                        <md-table-cell>{{ban.AdditionalInfo}}</md-table-cell>
                        <md-table-cell>
                            <i class="material-icons" @click="removeBan(banIndex,ban.ID)">delete_forever</i>
                        </md-table-cell>
                    </md-table-row>
                </md-table-body>
            </md-table>
            <sn-loading v-if="loadingResults"></sn-loading>
            <md-table-pagination :md-size="rowsPerPage" :md-total="totalRows" :md-page="currentPage" md-label="Rows" md-separator="of" :md-page-options="[10, 25, 50]" @pagination="changePage"></md-table-pagination>
    
        </div>
    </div>
</template>
<script>
import Paginator from './paginator.vue';
import LoadingSpinner from './loadingSpinner.vue';
import axios from 'axios';
import _ from 'lodash';

export default {
  components:{'paginator':Paginator, 'sn-loading': LoadingSpinner},
  data(){
      return{
          rowsPerPage: 25,
          currentPage : 1,
          totalRows: 0,
          searchSubs: [],
          loadingResults: false,
          searchResults: [],
          searchTerm: "",
          sort: 'date',
          ascending: false,
          subreddits: this.$select('snoonotes_info.modded_subs as subreddits')
      }
  },
  computed:{
      adminSubs: function(){
        return this.subreddits.filter((s)=>{return s.IsAdmin})
      }
  },
  methods:{
      toggleAll(){
          if(this.searchSubs.length == this.adminSubs.length){
              this.searchSubs = [];
          }
          else{
              this.searchSubs = this.adminSubs.map((s)=>{return s.SubName});
          }
      },
      searchBannedUsers:_.debounce(function(){
          this.loadingResults = true;
          let queryString = "";
          if(this.searchSubs.length != this.adminSubs.length){
              queryString += "subreddits=" + this.searchSubs.join() + "&";
          }
          if(this.searchTerm && this.searchTerm.length > 0){
              queryString += "searchterm="+this.searchTerm + "&";
          }
          queryString += "limit="+this.rowsPerPage +"&page="+this.currentPage+"&orderby="+this.sort+"&ascending="+this.ascending;
          //TODO other params
          axios.get('BotBan/Search/User?' + queryString).then((response)=>{
              this.loadingResults = false;
              this.searchResults = []; //just in case errors
              let results = response.data;
              this.currentPage = results.CurrentPage;
              this.rowsPerPage = results.ResultsPerPage;
              this.totalRows = results.TotalResults;
              this.searchResults = results.DataTable;
          },(err)=>{this.searchResults = []; this.loadingResults = false;})
      },500),
      sortTable(e){
          this.currentPage = 1;
          this.sort = e.name;
          this.ascending = e.type === "asc";
          this.loadingResults = true;
          this.searchBannedUsers();
      },
      changePage(e){
          this.rowsPerPage = e.size;
          this.currentPage = e.page;
          this.loadingResults = true;
          this.searchBannedUsers();
      },
      removeBan(index,banID){
          axios.delete('BotBan/'+this.searchResults[index].SubName+'/User/'+banID)
            .then((success)=>{
                this.searchResults.splice(index,1);
            })
      }
  },
  watch:{
      'searchSubs': function(){
          this.loadingResults = true;
          this.searchBannedUsers();
      },
      'searchTerm':function(){
          this.loadingResults = true;
          this.searchBannedUsers();
      }
  },
  created: function(){
      this.toggleAll();
      this.searchBannedUsers();
      
  },
  mounted:function(){
      document.body.classList.remove('md-theme-default');
  }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";

#SNOptionsPanel {
    .sn-search-sub-label {
        display: inline-block;
        vertical-align: center;
        background-color: white; //height:25px;
        min-width: 50px;
        padding: 10px;
        margin: 5px 10px;
        border: 1px solid $light-gray;
        border-radius: 5px;
        cursor: pointer;
        transition: background-color linear .3s;

        &.checked {
            background-color: $primary;
        }

        input[type="checkbox"] {
            display: none;
        }
    }

    .sn-toggle-all {
        background-color: $secondary;
        padding: 10px;
        border: 1px solid $light-gray;
        border-radius: 5px;
    }
    .sn-loading {
        margin: 0 auto;
        padding: 5px 0px;
    }
    .md-input-container.md-input-focused {
        textarea,
        i,
        label {
            color: $secondary;
        }
    }

    .md-table-row th{
        font-weight:bold;
    }
}
</style>
